using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using System.Linq;

namespace AjoCoreBackend.Application.Commands.IndividualContriution.LiquidateEarly
{
    public class LiquidateEarlyCommandHandler : IRequestHandler<LiquidateEarlyCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INombaApiClient _nombaApiClient;
        private readonly IBankCodeService _bankCodeService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public LiquidateEarlyCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INombaApiClient nombaApiClient,
            IBankCodeService bankCodeService,
            IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _nombaApiClient = nombaApiClient;
            _bankCodeService = bankCodeService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<bool> Handle(LiquidateEarlyCommand request, CancellationToken cancellationToken)
        {
            var userIdString = _currentUserService.UserId;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated properly.");
            }

            var cycle = await _unitOfWork.Repository<SavingCycle>().GetByIdAsync(request.SavingCycleId);

            if (cycle == null)
            {
                throw new NotFoundException($"Saving Cycle with ID {request.SavingCycleId} not found.");
            }
            
            var cycleMembers = await _unitOfWork.Repository<SavingCycleMember>().FindAsync(m => m.SavingCycleId == cycle.Id);
            cycle.Members = cycleMembers.ToList();

            if (cycle.CycleType != CycleType.Personal)
            {
                throw new DomainException("Only Personal savings cycles can be liquidated early.");
            }

            if (cycle.Status == CycleStatus.Completed)
            {
                throw new DomainException("This cycle is already completed and liquidated.");
            }

            var member = cycle.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null)
            {
                throw new UnauthorizedAccessException("You are not the owner of this personal savings cycle.");
            }

            // Check if already paid out
            var existingPayout = await _unitOfWork.Repository<PayoutLedger>()
                .FindAsync(p => p.SavingCycleMemberId == member.Id);
                
            if (existingPayout.Any())
            {
                throw new DomainException("This cycle has already been paid out.");
            }

            // Calculate total contributions
            var contributions = await _unitOfWork.Repository<ContributionLedger>()
                .FindAsync(c => c.SavingCycleMemberId == member.Id);
            
            var totalPayout = contributions.Sum(c => c.Amount);
            if (totalPayout <= 0)
            {
                throw new DomainException("No funds to liquidate.");
            }

            // Apply early liquidation penalty (e.g., 5% penalty, we can adjust this later)
            decimal penalty = totalPayout * 0.05m;
            totalPayout -= penalty;

            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(userId);
            if (trader == null || string.IsNullOrWhiteSpace(trader.PayoutAccountNumber) || string.IsNullOrWhiteSpace(trader.PayoutBankName))
            {
                throw new DomainException("You must configure your payout account details before liquidating.");
            }

            var actualBankCode = await _bankCodeService.GetBankCodeByNameAsync(trader.PayoutBankName);
            
            var lookupRequest = new Application.DTOs.Nomba.BankLookupRequest
            {
                AccountNumber = trader.PayoutAccountNumber,
                BankCode = actualBankCode
            };

            var lookupResponse = await _nombaApiClient.LookupBankAccountAsync(lookupRequest);
            if (string.IsNullOrWhiteSpace(lookupResponse.AccountName))
            {
                throw new DomainException("Bank lookup failed. Please check your payout account details.");
            }

            var now = _dateTimeProvider.UtcNow;
            var merchantTxRef = $"payout_early_{member.Id}_{now:yyyyMMddHHmmss}";

            var transferRequest = new Application.DTOs.Nomba.BankTransferRequest
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
                    SavingCycleMemberId = member.Id,
                    Amount = totalPayout,
                    MerchantTxRef = merchantTxRef,
                    PayoutDate = now
                };
                await _unitOfWork.Repository<PayoutLedger>().AddAsync(payoutLedger);

                cycle.Status = CycleStatus.Completed;
                _unitOfWork.Repository<SavingCycle>().Update(cycle);
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }

            throw new DomainException("Failed to execute the bank transfer. Please try again later.");
        }
    }
}
