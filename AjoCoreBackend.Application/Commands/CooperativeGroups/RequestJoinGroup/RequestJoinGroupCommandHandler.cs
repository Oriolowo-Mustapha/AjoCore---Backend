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

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.RequestJoinGroup
{
    public class RequestJoinGroupCommandHandler : IRequestHandler<RequestJoinGroupCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public RequestJoinGroupCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(RequestJoinGroupCommand request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.UserId 
                ?? throw new ForbiddenAccessException("You must be logged in."));

            // Verify group exists
            var group = await _unitOfWork.Repository<CooperativeGroup>().GetByIdAsync(request.GroupId);
            if (group == null)
            {
                throw new NotFoundException($"Cooperative group with ID {request.GroupId} not found.");
            }

            if (group.CooperativeAdminId == userId)
            {
                throw new DomainException("Admin cannot request to join their own group.");
            }

            // Check if already a member or has a pending request
            var existing = await _unitOfWork.Repository<CooperativeGroupMember>()
                .FindAsync(m => m.CooperativeGroupId == request.GroupId && m.TraderId == userId);

            if (existing.Any())
            {
                var status = existing.First().Status;
                throw new DomainException($"You already have a {status} membership request for this group.");
            }

            var membership = new CooperativeGroupMember
            {
                CooperativeGroupId = request.GroupId,
                TraderId = userId,
                Status = ApprovalStatus.Pending
            };

            await _unitOfWork.Repository<CooperativeGroupMember>().AddAsync(membership);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return membership.Id;
        }
    }
}
