using System;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.RejectGroupMember
{
    public class RejectGroupMemberCommandHandler : IRequestHandler<RejectGroupMemberCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public RejectGroupMemberCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(RejectGroupMemberCommand request, CancellationToken cancellationToken)
        {
            var adminId = Guid.Parse(_currentUserService.UserId 
                ?? throw new ForbiddenAccessException("You must be logged in."));

            var membership = await _unitOfWork.Repository<CooperativeGroupMember>()
                .GetByIdAsync(request.MembershipId);

            if (membership == null)
            {
                throw new NotFoundException($"Membership request {request.MembershipId} not found.");
            }

            var group = await _unitOfWork.Repository<CooperativeGroup>()
                .GetByIdAsync(membership.CooperativeGroupId);

            if (group == null || group.AdminTraderId != adminId)
            {
                throw new ForbiddenAccessException("Only the group admin can reject membership requests.");
            }

            if (membership.Status != ApprovalStatus.Pending)
            {
                throw new DomainException($"This request has already been {membership.Status}.");
            }

            membership.Status = ApprovalStatus.Rejected;

            _unitOfWork.Repository<CooperativeGroupMember>().Update(membership);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
