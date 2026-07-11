using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetGroupMembers
{
    public class GetGroupMembersQueryHandler : IRequestHandler<GetGroupMembersQuery, List<GroupMemberDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetGroupMembersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<GroupMemberDto>> Handle(GetGroupMembersQuery request, CancellationToken cancellationToken)
        {
            var group = await _unitOfWork.Repository<CooperativeGroup>().GetByIdAsync(request.GroupId);
            if (group == null)
            {
                throw new NotFoundException($"Group {request.GroupId} not found.");
            }

            var members = await _unitOfWork.Repository<CooperativeGroupMember>()
                .FindAsync(m => m.CooperativeGroupId == request.GroupId && m.Status == ApprovalStatus.Approved);

            var result = new List<GroupMemberDto>();

            foreach (var member in members)
            {
                var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(member.TraderId);

                result.Add(new GroupMemberDto
                {
                    Id = member.Id,
                    TraderId = member.TraderId,
                    TraderName = trader != null ? $"{trader.FirstName} {trader.LastName}" : "Unknown",
                    TraderEmail = trader?.Email ?? "",
                    TraderPhone = trader?.PhoneNumber ?? "",
                    Status = member.Status.ToString(),
                    CreatedAt = member.CreatedAt,
                    ApprovedAt = member.ApprovedAt
                });
            }

            return result;
        }
    }
}
