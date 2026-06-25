using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AjoCoreBackend.Infrastructure.BackgroundJobs
{
    public class LiquidationSweepService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LiquidationSweepService> _logger;

        public LiquidationSweepService(IServiceProvider serviceProvider, ILogger<LiquidationSweepService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Liquidation Sweep Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessLiquidationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing Liquidation Sweep.");
                }

                // Run every 12 hours (can be configured via appsettings in a real scenario)
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task ProcessLiquidationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var nombaApiClient = scope.ServiceProvider.GetRequiredService<INombaApiClient>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var now = dateTimeProvider.UtcNow;

            // Fetch active cycles that are Rotational (ROSCA)
            var activeCycles = await unitOfWork.SavingCycles.FindAsync(c => c.Status == CycleStatus.Active && c.CycleType == CycleType.Rosca);

            foreach (var cycle in activeCycles)
            {
                var cycleWithMembers = await unitOfWork.SavingCycles.GetCycleWithMembersAsync(cycle.Id);
                if (cycleWithMembers == null) continue;

                // Simple interval calculation based on StartDate and IntervalDays
                var daysSinceStart = (now - cycle.StartDate).TotalDays;
                var currentInterval = (int)(daysSinceStart / cycle.IntervalDays) + 1;

                if (currentInterval > cycle.Members.Count)
                {
                    // Cycle has completed all intervals
                    cycle.Status = CycleStatus.Completed;
                    unitOfWork.SavingCycles.Update(cycle);
                    continue;
                }

                // Identify member for current payout order
                var payoutMember = cycleWithMembers.Members.FirstOrDefault(m => m.PayoutOrder == currentInterval);
                if (payoutMember == null || payoutMember.VirtualAccount == null) continue;

                // Check if we already processed a payout for this specific interval via Idempotent Ref
                var merchantTxRef = $"payout_cycle{cycle.Id:N}_interval{currentInterval}";
                var ledgers = await unitOfWork.Repository<PayoutLedger>().FindAsync(l => l.MerchantTxRef == merchantTxRef);
                if (ledgers.Any())
                {
                    continue; // Already paid out
                }

                // Execute Payout
                // For a ROSCA, the payout amount is ContributionAmount * MemberCount
                var totalPayout = cycle.ContributionAmount * cycleWithMembers.Members.Count;
                
                var transferRequest = new Application.DTOs.Nomba.BankTransferRequest
                {
                    Amount = totalPayout,
                    AccountNumber = payoutMember.VirtualAccount.AccountNumber ?? "",
                    AccountName = payoutMember.VirtualAccount.AccountName ?? "AjoCore Member",
                    BankCode = payoutMember.VirtualAccount.BankName ?? "033", // Dummy mapping for now
                    MerchantTxRef = merchantTxRef,
                    SenderName = "AjoCore Thrift"
                };

                var transferResponse = await nombaApiClient.ExecuteBankTransferAsync(transferRequest);

                if (transferResponse != null) // If successful
                {
                    var payoutLedger = new PayoutLedger
                    {
                        SavingCycleMemberId = payoutMember.Id,
                        Amount = totalPayout,
                        MerchantTxRef = merchantTxRef,
                        PayoutDate = now
                    };
                    await unitOfWork.Repository<PayoutLedger>().AddAsync(payoutLedger);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
