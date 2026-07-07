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
        private readonly IEmailService _emailService;

        public LiquidationSweepService(
            IUnitOfWork unitOfWork,
            INombaApiClient nombaApiClient,
            IBankCodeService bankCodeService,
            IDateTimeProvider dateTimeProvider,
            ILogger<LiquidationSweepService> logger,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _nombaApiClient = nombaApiClient;
            _bankCodeService = bankCodeService;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _emailService = emailService;
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
                    try
                    {
                        if (cycle.CycleType == CycleType.Rosca)
                        {
                        var cycleWithMembers = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(cycle.Id);
                        if (cycleWithMembers == null) continue;

                        // Simple interval calculation based on StartDate and IntervalDays
                        var daysSinceStart = (now - cycle.StartDate.Value).TotalDays;
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

                        var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(payoutMember.UserId);
                        if (trader == null || string.IsNullOrWhiteSpace(trader.PayoutAccountNumber) || string.IsNullOrWhiteSpace(trader.PayoutBankName))
                        {
                            _logger.LogWarning($"No payout account configured for Trader {payoutMember.UserId}. Skipping payout.");
                            continue;
                        }

                        var actualBankCode = await _bankCodeService.GetBankCodeByNameAsync(trader.PayoutBankName);
                        
                        var lookupRequest = new Application.DTOs.Nomba.BankLookupRequest
                        {
                            AccountNumber = trader.PayoutAccountNumber,
                            BankCode = actualBankCode
                        };

                        Application.DTOs.Nomba.BankLookupResponse lookupResponse = null;
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
                            AccountNumber = trader.PayoutAccountNumber,
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

                            // Broadcast notification to all members of the cycle
                            try
                            {
                                var memberIds = cycleWithMembers.Members.Select(m => m.UserId).ToList();
                                foreach (var memberId in memberIds)
                                {
                                    var memberTrader = await _unitOfWork.Repository<Trader>().GetByIdAsync(memberId);
                                    if (memberTrader != null && !string.IsNullOrWhiteSpace(memberTrader.Email))
                                    {
                                        string subject = $"Payout Completed for {cycle.Name}";
                                        string body = $"<p>Hello {memberTrader.FirstName},</p><p>We are pleased to inform you that <strong>{trader.FirstName} {trader.LastName}</strong> has successfully received the payout for Round {currentInterval} of the <strong>{cycle.Name}</strong> cycle.</p><p>Thank you for saving with AjoCore.</p>";
                                        
                                        // Fire and forget so we don't block the sweep
                                        _ = _emailService.SendEmailAsync(memberTrader.Email, subject, body, true); 
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Failed to send broadcast notifications for cycle {cycle.Id}, interval {currentInterval}.");
                            }
                        }
                    }
                    else if (cycle.CycleType == CycleType.Asca)
                    {
                        await ProcessLiquidationAsyncForAsca(cycle);
                    }
                    else if (cycle.CycleType == CycleType.Personal)
                    {
                        await ProcessLiquidationAsyncForPersonal(cycle);
                    }

                    await _unitOfWork.SaveChangesAsync(default);
                }
                catch (Exception cycleEx)
                {
                    _logger.LogError(cycleEx, $"Error processing cycle {cycle.Id}");
                }
            }
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
            
            if (cycle.EndDate == null || cycle.EndDate.Value > now) return;

            var cycleWithMembers = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(cycle.Id);
            if (cycleWithMembers == null) return;
            
            foreach (var member in cycleWithMembers.Members)
            {
                if (member.VirtualAccount == null) continue;
                
                var merchantTxRef = $"payout_cycle{cycle.Id:N}_member{member.Id:N}";
                var ledgers = await _unitOfWork.Repository<PayoutLedger>().FindAsync(l => l.MerchantTxRef == merchantTxRef);
                if (ledgers.Any())
                {
                    continue; // Already paid out
                }

                var contributions = await _unitOfWork.Repository<ContributionLedger>().FindAsync(c => c.SavingCycleMemberId == member.Id);
                var totalPayout = contributions.Sum(c => c.Amount);
                if (totalPayout <= 0) continue;
                
                var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(member.UserId);
                if (trader == null || string.IsNullOrWhiteSpace(trader.PayoutAccountNumber) || string.IsNullOrWhiteSpace(trader.PayoutBankName))
                {
                    _logger.LogWarning($"No payout account configured for Trader {member.UserId}. Skipping payout.");
                    continue;
                }

                var actualBankCode = await _bankCodeService.GetBankCodeByNameAsync(trader.PayoutBankName);
                
                var lookupRequest = new Application.DTOs.Nomba.BankLookupRequest
                {
                    AccountNumber = trader.PayoutAccountNumber,
                    BankCode = actualBankCode
                };

                Application.DTOs.Nomba.BankLookupResponse lookupResponse = null;
                try
                {
                    lookupResponse = await _nombaApiClient.LookupBankAccountAsync(lookupRequest);
                    if (string.IsNullOrWhiteSpace(lookupResponse.AccountName))
                    {
                        _logger.LogWarning($"Bank lookup failed for Account {lookupRequest.AccountNumber}, BankCode {lookupRequest.BankCode}");
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred while looking up bank account for Account {lookupRequest.AccountNumber}, BankCode {lookupRequest.BankCode}");
                    continue;
                }

                if (lookupResponse == null || string.IsNullOrWhiteSpace(lookupResponse.AccountName))
                {
                    _logger.LogWarning($"Bank lookup failed for Account {lookupRequest.AccountNumber}, BankCode {lookupRequest.BankCode}");
                    continue;
                }
                
                var transferRequest = new BankTransferRequest
                {
                    Amount = totalPayout,
                    AccountNumber = trader.PayoutAccountNumber,
                    AccountName = lookupResponse.AccountName ?? "",
                    BankCode = lookupRequest.BankCode,
                    MerchantTxRef = merchantTxRef,
                    SenderName = "AjoCore Thrift"
                };

                var transferResponse = await _nombaApiClient.ExecuteBankTransferAsync(transferRequest);

                if (transferResponse != null)
                {
                    try
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
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Database error recording payout {merchantTxRef} for Merchant {member.Id}. Manual resolution required.");
                    }
                }
            }

            cycle.Status = CycleStatus.Completed;
            _unitOfWork.SavingCycles.Update(cycle);
        }

        private async Task ProcessLiquidationAsyncForPersonal(SavingCycle cycle)
        {
            var now = _dateTimeProvider.UtcNow;
            
            // For Personal, we only liquidate when EndDate is reached
            if (cycle.EndDate == null || cycle.EndDate.Value > now)
            {
                return;
            }

            var cycleWithMembers = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(cycle.Id);

            if (cycleWithMembers == null || !cycleWithMembers.Members.Any()) return;

            var member = cycleWithMembers.Members.First();
            
            // Check if already paid out
            var existingPayout = await _unitOfWork.Repository<PayoutLedger>()
                .FindAsync(p => p.SavingCycleMemberId == member.Id);
                
            if (existingPayout.Any()) return;

            // Calculate total contributions
            var contributions = await _unitOfWork.Repository<ContributionLedger>()
                .FindAsync(c => c.SavingCycleMemberId == member.Id);
            
            var totalPayout = contributions.Sum(c => c.Amount);
            if (totalPayout <= 0) return;

            var merchantTxRef = $"payout_per_{member.Id}_{now:yyyyMMddHHmmss}";

            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(member.UserId);
            if (trader == null || string.IsNullOrWhiteSpace(trader.PayoutAccountNumber) || string.IsNullOrWhiteSpace(trader.PayoutBankName))
            {
                _logger.LogWarning($"No payout account configured for Trader {member.UserId}. Skipping payout.");
                return;
            }

            var actualBankCode = await _bankCodeService.GetBankCodeByNameAsync(trader.PayoutBankName);
            
            var lookupRequest = new Application.DTOs.Nomba.BankLookupRequest
            {
                AccountNumber = trader.PayoutAccountNumber,
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

            var transferRequest = new BankTransferRequest
            {
                Amount = totalPayout,
                AccountNumber = trader.PayoutAccountNumber,
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

                // Mark the cycle as completed
                cycle.Status = CycleStatus.Completed;
                _unitOfWork.Repository<SavingCycle>().Update(cycle);
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            }
        }
    }
}
