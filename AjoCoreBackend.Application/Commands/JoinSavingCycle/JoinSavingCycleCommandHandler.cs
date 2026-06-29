using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Nomba;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.JoinSavingCycle
{
    public class JoinSavingCycleCommandHandler : IRequestHandler<JoinSavingCycleCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INombaApiClient _nombaApiClient;
        private readonly ICurrentUserService _currentUserService;

        public JoinSavingCycleCommandHandler(
            IUnitOfWork unitOfWork,
            INombaApiClient nombaApiClient,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _nombaApiClient = nombaApiClient;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(JoinSavingCycleCommand request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.UserId 
                ?? throw new ForbiddenAccessException("You must be logged in."));

            var cycle = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(request.SavingCycleId);

            if (cycle == null)
            {
                throw new NotFoundException($"Saving Cycle with ID {request.SavingCycleId} was not found.");
            }

            if (cycle.Status != CycleStatus.Pending)
            {
                throw new CycleAlreadyStartedException(cycle.Id);
            }

            if (cycle.Members.Any(m => m.UserId == userId))
            {
                throw new MemberAlreadyExistsException(userId, cycle.Id);
            }

            if (cycle.CooperativeGroupId.HasValue)
            {
                var groupMemberships = await _unitOfWork.Repository<CooperativeGroupMember>()
                    .FindAsync(m => m.CooperativeGroupId == cycle.CooperativeGroupId.Value && m.TraderId == userId);
                
                var membership = groupMemberships.FirstOrDefault();
                if (membership == null || membership.Status != ApprovalStatus.Approved)
                {
                    throw new ForbiddenAccessException("You must be an approved member of the Cooperative Group to join this saving cycle.");
                }
            }

            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(userId);
            if (trader == null) throw new NotFoundException($"Trader with ID {userId} not found.");

            // Provision a NUBAN Virtual Account attached to the cycle's Nomba Sub-Account
            var virtualAccountResponse = await _nombaApiClient.CreateVirtualAccountAsync(new CreateVirtualAccountRequest
            {
                AccountReference = $"member_{userId:N}",
                AccountName = $"{trader.FirstName} {trader.LastName} AjoCore"
            });

            var virtualAccount = new NombaVirtualAccount
            {
                AccountNumber = virtualAccountResponse.AccountNumber,
                BankName = virtualAccountResponse.BankName,
                AccountName = virtualAccountResponse.AccountName
            };

            var highestOrder = await _unitOfWork.SavingCycleMembers.GetHighestPayoutOrderAsync(request.SavingCycleId);

            var member = new SavingCycleMember
            {
                SavingCycleId = request.SavingCycleId,
                UserId = userId,
                Status = MemberStatus.Active,
                PayoutOrder = highestOrder + 1,
                VirtualAccount = virtualAccount
            };

            await _unitOfWork.SavingCycleMembers.AddAsync(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return member.Id;
        }
    }
}
