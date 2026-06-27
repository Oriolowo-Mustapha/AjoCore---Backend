using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetAllGroups
{
    public class GetAllGroupsQueryHandler : IRequestHandler<GetAllGroupsQuery, List<CooperativeGroupDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllGroupsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CooperativeGroupDto>> Handle(GetAllGroupsQuery request, CancellationToken cancellationToken)
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

            var result = new List<CooperativeGroupDto>();

            foreach (var group in groups)
            {
                var admin = await _unitOfWork.Repository<CooperativeAdmin>().GetByIdAsync(group.CooperativeAdminId);
                var members = await _unitOfWork.Repository<CooperativeGroupMember>()
                    .FindAsync(m => m.CooperativeGroupId == group.Id && m.Status == ApprovalStatus.Approved);
                var cycles = await _unitOfWork.Repository<SavingCycle>()
                    .FindAsync(c => c.CooperativeGroupId == group.Id);

                result.Add(new CooperativeGroupDto
                {
                    Id = group.Id,
                    Name = group.Name,
                    Description = group.Description,
                    CooperativeAdminId = group.CooperativeAdminId,
                    AdminName = admin != null ? $"{admin.FirstName} {admin.LastName}" : "Unknown",
                    MemberCount = members.Count(),
                    CycleCount = cycles.Count(),
                    CreatedAt = group.CreatedAt
                });
            }

            return result;
        }
    }

    public class CooperativeGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid CooperativeAdminId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int CycleCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
