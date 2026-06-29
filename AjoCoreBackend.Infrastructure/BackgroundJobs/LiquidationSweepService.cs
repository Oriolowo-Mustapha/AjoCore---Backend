using AjoCoreBackend.Application.DTOs.Nomba;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AjoCoreBackend.Infrastructure.BackgroundJobs
{
    public class LiquidationSweepService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INombaApiClient _nombaApiClient;
        private readonly IBankCodeService _bankCodeService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<LiquidationSweepService> _logger;

        public LiquidationSweepService(
            IUnitOfWork unitOfWork,
            INombaApiClient nombaApiClient,
            IBankCodeService bankCodeService,
            IDateTimeProvider dateTimeProvider,
            ILogger<LiquidationSweepService> logger)
        {
            _unitOfWork = unitOfWork;
            _nombaApiClient = nombaApiClient;
            _bankCodeService = bankCodeService;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task ProcessLiquidationsAsync()
        {
            _logger.LogInformation("Hangfire Job: Liquidation Sweep Service is executing.");
            try
            {
                var now = _dateTimeProvider.UtcNow;

                // Fetch active cycles that are Rotational (ROSCA) or ASCA
                var activeCycles = await _unitOfWork.SavingCycles.FindAsync(c => c.Status == CycleStatus.Active && (c.CycleType == CycleType.Rosca || c.CycleType == CycleType.Asca));

                foreach (var cycle in activeCycles)
                {
                    if (cycle.CycleType == CycleType.Rosca)
                    {
                        var cycleWithMembers = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(cycle.Id);
                        if (cycleWithMembers == null) continue;

                        // Simple interval calculation based on StartDate and IntervalDays
                        var daysSinceStart = (now - cycle.StartDate).TotalDays;
                        var currentInterval = (int)(daysSinceStart / cycle.IntervalDays) + 1;

                        if (currentInterval > cycle.Members.Count)
                        {
                            // Cycle has completed all intervals
                            cycle.Status = CycleStatus.Completed;
                            _unitOfWork.SavingCycles.Update(cycle);
                            continue;
                        }

                        // Identify member for current payout order
                        var payoutMember = cycleWithMembers.Members.FirstOrDefault(m => m.PayoutOrder == currentInterval);
                        if (payoutMember == null || payoutMember.VirtualAccount == null) continue;

                        // Check if we already processed a payout for this specific interval via Idempotent Ref
                        var merchantTxRef = $"payout_cycle{cycle.Id:N}_interval{currentInterval}";
                        var ledgers = await _unitOfWork.Repository<PayoutLedger>().FindAsync(l => l.MerchantTxRef == merchantTxRef);
                        if (ledgers.Any())
                        {
                            continue; // Already paid out
                        }

                        // Execute Payout
                        // For a ROSCA, the payout amount is ContributionAmount * MemberCount
                        var totalPayout = cycle.ContributionAmount * cycleWithMembers.Members.Count;

                        // Get the actual BankCode from the BankName using our cached service
                        var actualBankCode = await _bankCodeService.GetBankCodeByNameAsync(payoutMember.VirtualAccount.BankName ?? "");

                        if (string.IsNullOrEmpty(actualBankCode))
                        {
                            _logger.LogWarning($"Could not resolve BankCode for {payoutMember.VirtualAccount.BankName}. Skipping payout {merchantTxRef}.");
                            continue;
                        }

                        var lookupRequest = new Application.DTOs.Nomba.BankLookupRequest
                        {
                            AccountNumber = payoutMember.VirtualAccount.AccountNumber ?? "",
                            BankCode = actualBankCode
                        };

                        Application.DTOs.Nomba.BankLookupResponse lookupResponse;
                        try
                        {
                            lookupResponse = await _nombaApiClient.LookupBankAccountAsync(lookupRequest);

                            if (string.IsNullOrWhiteSpace(lookupResponse.AccountName))
                            {
                                _logger.LogWarning($"Bank lookup failed for Account {lookupRequest.AccountNumber}, BankCode {lookupRequest.BankCode}. Skipping payout {merchantTxRef}.");
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error validating bank account for payout {merchantTxRef}. Skipping.");
                            continue;
                        }

                        var transferRequest = new BankTransferRequest
                        {
                            Amount = totalPayout,
                            AccountNumber = payoutMember.VirtualAccount.AccountNumber ?? "",
                            AccountName = lookupResponse.AccountName,
                            BankCode = lookupRequest.BankCode,
                            MerchantTxRef = merchantTxRef,
                            SenderName = "AjoCore Thrift"
                        };

                        var transferResponse = await _nombaApiClient.ExecuteBankTransferAsync(transferRequest);

                        if (transferResponse != null)
                        {
                            var payoutLedger = new PayoutLedger
                            {
                                SavingCycleMemberId = payoutMember.Id,
                                Amount = totalPayout,
                                MerchantTxRef = merchantTxRef,
                                PayoutDate = now
                            };
                            await _unitOfWork.Repository<PayoutLedger>().AddAsync(payoutLedger);
                        }
                    }
                    else if (cycle.CycleType == CycleType.Asca)
                    {
                        await ProcessLiquidationAsyncForAsca(cycle);
                    }
                }

                await _unitOfWork.SaveChangesAsync(default);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Hangfire Liquidation Sweep.");
                throw;
            }
        }


        private async Task ProcessLiquidationAsyncForAsca(SavingCycle cycle)
        {
            var now = _dateTimeProvider.UtcNow;
            
            // For ASCA, we can process payouts for all members at once
            var cycleWithMembers = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(cycle.Id);
            if (cycleWithMembers == null) return;
            var member = cycleWithMembers.Members.FirstOrDefault();
            if (member == null || member.VirtualAccount == null) return;
            
            // Check if we already processed a payout for this member via Idempotent Ref
            var merchantTxRef = $"payout_cycle{cycle.Id:N}_member{member.Id:N}";
            var ledgers = await _unitOfWork.Repository<PayoutLedger>().FindAsync(l => l.MerchantTxRef == merchantTxRef);
            if (ledgers.Any())
            {
                return; // Already paid out
            }

            // Execute Payout
            var totalPayout = cycle.IndividualTargetAmount;
            
            // Get the actual BankCode from the BankName using our cached service
            var actualBankCode = await _bankCodeService.GetBankCodeByNameAsync(member.VirtualAccount.BankName ?? "");
            if (string.IsNullOrEmpty(actualBankCode))
            {
                _logger.LogWarning($"Could not resolve BankCode for {member.VirtualAccount.BankName}. Skipping payout {merchantTxRef}.");
                return;
            }
            
            // Lookup bank name from Nomba before transfer (as per spec)
            var lookupRequest = new Application.DTOs.Nomba.BankLookupRequest
            {
                AccountNumber = member.VirtualAccount.AccountNumber ?? "",
                BankCode = actualBankCode
            };

            Application.DTOs.Nomba.BankLookupResponse lookupResponse = null;
            try
            {
                lookupResponse = await _nombaApiClient.LookupBankAccountAsync(lookupRequest);
                if (string.IsNullOrWhiteSpace(lookupResponse.AccountName))
                {
                    _logger.LogWarning($"Bank lookup failed for Account {lookupRequest.AccountNumber}, BankCode {lookupRequest.BankCode}");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while looking up bank account for Account {lookupRequest.AccountNumber}, BankCode {lookupRequest.BankCode}");
                return;
            }

            if(lookupResponse == null || string.IsNullOrWhiteSpace(lookupResponse.AccountName))
            {
                _logger.LogWarning($"Bank lookup failed for Account {lookupRequest.AccountNumber}, BankCode {lookupRequest.BankCode}");
                return;
            }
            
            var transferRequest = new BankTransferRequest
            {
                Amount = totalPayout,
                AccountNumber = member.VirtualAccount.AccountNumber ?? "",
                AccountName = lookupResponse.AccountName ?? "",
                BankCode = lookupRequest.BankCode,
                MerchantTxRef = merchantTxRef,
                SenderName = "AjoCore Thrift"
            };

            var transferResponse = await _nombaApiClient.ExecuteBankTransferAsync(transferRequest);

            if (transferResponse != null)
            {
                var payoutLedger = new PayoutLedger
                {
                    SavingCycleMemberId = member.Id,
                    Amount = totalPayout,
                    MerchantTxRef = merchantTxRef,
                    PayoutDate = now
                };
                await _unitOfWork.Repository<PayoutLedger>().AddAsync(payoutLedger);
            }
        }
    }
}
