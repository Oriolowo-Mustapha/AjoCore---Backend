using System;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.ApproveGroupMember
{
    public class ApproveGroupMemberCommandHandler : IRequestHandler<ApproveGroupMemberCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ApproveGroupMemberCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<bool> Handle(ApproveGroupMemberCommand request, CancellationToken cancellationToken)
        {
            var adminId = Guid.Parse(_currentUserService.UserId 
                ?? throw new ForbiddenAccessException("You must be logged in."));

            var membership = await _unitOfWork.Repository<CooperativeGroupMember>()
                .GetByIdAsync(request.MembershipId);

            if (membership == null)
            {
                throw new NotFoundException($"Membership request {request.MembershipId} not found.");
            }

            // Verify the caller is the admin of this group
            var group = await _unitOfWork.Repository<CooperativeGroup>()
                .GetByIdAsync(membership.CooperativeGroupId);

            if (group == null)
            {
                throw new NotFoundException($"Membership with ID {request.MembershipId} was not found.");
            }

            if (group.CooperativeAdminId != adminId)
            {
                throw new ForbiddenAccessException("Only the group admin can approve membership requests.");
            }

            if (membership.Status != ApprovalStatus.Pending)
            {
                throw new DomainException($"This request has already been {membership.Status}.");
            }

            membership.Status = ApprovalStatus.Approved;
            membership.ApprovedAt = _dateTimeProvider.UtcNow;

            _unitOfWork.Repository<CooperativeGroupMember>().Update(membership);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
