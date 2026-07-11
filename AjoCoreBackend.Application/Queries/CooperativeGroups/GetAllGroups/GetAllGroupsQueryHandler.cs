using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Application.Interfaces.Services;
using MediatR;
using System;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetAllGroups
{
    public class GetAllGroupsQueryHandler : IRequestHandler<GetAllGroupsQuery, List<CooperativeGroupDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetAllGroupsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<AjoCoreBackend.Application.DTOs.CooperativeGroupDto>> Handle(GetAllGroupsQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<CooperativeGroup> groups;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                groups = await _unitOfWork.Repository<CooperativeGroup>()
                    .FindAsync(g => g.Name.ToLower().Contains(term) || g.Description.ToLower().Contains(term));
            }
            else
            {
                groups = await _unitOfWork.Repository<CooperativeGroup>().GetAllAsync();
            }

            var result = new List<AjoCoreBackend.Application.DTOs.CooperativeGroupDto>();

            foreach (var group in groups)
            {
                var admin = await _unitOfWork.Repository<CooperativeAdmin>().GetByIdAsync(group.CooperativeAdminId);
                var members = await _unitOfWork.Repository<CooperativeGroupMember>()
                    .FindAsync(m => m.CooperativeGroupId == group.Id && m.Status == ApprovalStatus.Approved);
                var cycles = await _unitOfWork.Repository<SavingCycle>()
                    .FindAsync(c => c.CooperativeGroupId == group.Id);

                // Fetch cycle members and their contributions for the TotalSaved calculation
                decimal totalSaved = 0;
                foreach (var cycle in cycles)
                {
                    var cycleMembers = await _unitOfWork.Repository<SavingCycleMember>().FindAsync(m => m.SavingCycleId == cycle.Id);
                    foreach(var cm in cycleMembers) {
                        var contributions = await _unitOfWork.Repository<ContributionLedger>().FindAsync(c => c.SavingCycleMemberId == cm.Id);
                        totalSaved += contributions.Sum(c => c.Amount);
                    }
                }

                string? membershipStatus = null;
                if (Guid.TryParse(_currentUserService.UserId, out var userId))
                {
                    var memberships = await _unitOfWork.Repository<CooperativeGroupMember>()
                        .FindAsync(m => m.CooperativeGroupId == group.Id && m.TraderId == userId);
                    var mem = memberships.FirstOrDefault();
                    if (mem != null)
                    {
                        membershipStatus = mem.Status.ToString();
                    }
                }

                result.Add(new AjoCoreBackend.Application.DTOs.CooperativeGroupDto
                {
                    Id = group.Id,
                    Name = group.Name,
                    Description = group.Description,
                    AdminTraderId = group.CooperativeAdminId,
                    AdminName = admin != null ? $"{admin.FirstName} {admin.LastName}" : "Unknown",
                    MemberCount = members.Count(),
                    CycleCount = cycles.Count(),
                    SavingsGoal = cycles.Sum(c => c.ContributionAmount), // Wait, is SavingsGoal ContributionAmount * members? The prompt says "sum of all ContributionAmount values across all cycles"
                    TotalSaved = totalSaved,
                    IsActive = group.Status == GroupStatus.Active,
                    CreatedAt = group.CreatedAt,
                    MembershipStatus = membershipStatus
                });
            }

            return result;
        }
    }
}
