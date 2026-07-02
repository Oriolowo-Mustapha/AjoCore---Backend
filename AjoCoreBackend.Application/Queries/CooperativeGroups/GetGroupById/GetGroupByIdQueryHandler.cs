using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetGroupById
{
    public class GetGroupByIdQueryHandler : IRequestHandler<GetGroupByIdQuery, CooperativeGroupDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetGroupByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CooperativeGroupDto> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var group = await _unitOfWork.Repository<CooperativeGroup>().GetByIdAsync(request.GroupId);
            
            if (group == null)
            {
                throw new NotFoundException($"Cooperative group with ID {request.GroupId} not found.");
            }

            var admin = await _unitOfWork.Repository<CooperativeAdmin>().GetByIdAsync(group.CooperativeAdminId);
            var members = await _unitOfWork.Repository<CooperativeGroupMember>()
                .FindAsync(m => m.CooperativeGroupId == group.Id && m.Status == ApprovalStatus.Approved);
            var cycles = await _unitOfWork.Repository<SavingCycle>()
                .FindAsync(c => c.CooperativeGroupId == group.Id);

            decimal totalSaved = 0;
            bool isActive = false;
            foreach (var cycle in cycles)
            {
                if (cycle.Status == CycleStatus.Active) isActive = true;
                var cycleMembers = await _unitOfWork.Repository<SavingCycleMember>().FindAsync(m => m.SavingCycleId == cycle.Id);
                foreach(var cm in cycleMembers) {
                    var contributions = await _unitOfWork.Repository<ContributionLedger>().FindAsync(c => c.SavingCycleMemberId == cm.Id);
                    totalSaved += contributions.Sum(c => c.Amount);
                }
            }

            return new CooperativeGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                AdminTraderId = group.CooperativeAdminId,
                AdminName = admin != null ? $"{admin.FirstName} {admin.LastName}" : "Unknown",
                MemberCount = members.Count(),
                CycleCount = cycles.Count(),
                SavingsGoal = cycles.Sum(c => c.ContributionAmount),
                TotalSaved = totalSaved,
                IsActive = isActive,
                CreatedAt = group.CreatedAt
            };
        }
    }
}
