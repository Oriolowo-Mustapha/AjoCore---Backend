using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetPendingRequests
{
    public class GetPendingRequestsQueryHandler : IRequestHandler<GetPendingRequestsQuery, List<GroupMemberDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetPendingRequestsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<GroupMemberDto>> Handle(GetPendingRequestsQuery request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.UserId 
                ?? throw new ForbiddenAccessException("You must be logged in."));

            var group = await _unitOfWork.Repository<CooperativeGroup>().GetByIdAsync(request.GroupId);
            if (group == null)
            {
                throw new NotFoundException($"Cooperative Group with ID {request.GroupId} was not found.");
            }

            if (group.CooperativeAdminId != userId)
            {
                throw new ForbiddenAccessException("Only the group admin can view pending requests.");
            }

            var pendingMembers = await _unitOfWork.Repository<CooperativeGroupMember>()
                .FindAsync(m => m.CooperativeGroupId == request.GroupId && m.Status == ApprovalStatus.Pending);

            var result = new List<GroupMemberDto>();

            foreach (var member in pendingMembers)
            {
                var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(member.TraderId);

                result.Add(new GroupMemberDto
                {
                    Id = member.Id,
                    TraderId = member.TraderId,
                    TraderName = trader != null ? $"{trader.FirstName} {trader.LastName}" : "Unknown",
                    TraderEmail = trader?.Email ?? "",
                    Status = member.Status.ToString(),
                    CreatedAt = member.CreatedAt,
                    ApprovedAt = member.ApprovedAt
                });
            }

            return result;
        }
    }
}
