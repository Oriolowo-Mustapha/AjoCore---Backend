using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.RemoveMember
{
    public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public RemoveMemberCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
        {
            var membership = await _unitOfWork.Repository<CooperativeGroupMember>().GetByIdAsync(request.MembershipId);
            if (membership == null || membership.CooperativeGroupId != request.GroupId)
            {
                throw new NotFoundException("Membership not found.");
            }

            var group = await _unitOfWork.Repository<CooperativeGroup>().GetByIdAsync(request.GroupId);
            if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId) || group.CooperativeAdminId != currentUserId)
            {
                throw new UnauthorizedAccessException("Only the group admin can remove members.");
            }

            var allCycles = await _unitOfWork.Repository<SavingCycle>()
                .FindAsync(c => c.CooperativeGroupId == request.GroupId);

            foreach (var cycle in allCycles)
            {
                var cycleMembers = await _unitOfWork.Repository<SavingCycleMember>()
                    .FindAsync(m => m.SavingCycleId == cycle.Id && m.UserId == membership.TraderId);

                if (cycleMembers.Any())
                {
                    if (cycle.Status == CycleStatus.Active)
                    {
                        throw new InvalidOperationException("Cannot remove member: they have active saving cycles in this group.");
                    }
                    else if (cycle.Status == CycleStatus.Completed)
                    {
                        foreach (var cm in cycleMembers)
                        {
                            var payouts = await _unitOfWork.Repository<PayoutLedger>()
                                .FindAsync(p => p.SavingCycleMemberId == cm.Id);
                            if (!payouts.Any())
                            {
                                throw new InvalidOperationException("Cannot remove member: they have unsettled payouts.");
                            }
                        }
                    }
                }
            }

            _unitOfWork.Repository<CooperativeGroupMember>().Delete(membership);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
