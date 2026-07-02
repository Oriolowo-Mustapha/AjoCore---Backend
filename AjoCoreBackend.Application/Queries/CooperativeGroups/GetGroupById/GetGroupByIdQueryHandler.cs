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
    public class GetGroupByIdQueryHandler : IRequestHandler<GetGroupByIdQuery, CooperativeGroupDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetGroupByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CooperativeGroupDetailDto> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var group = await _unitOfWork.Repository<CooperativeGroup>().GetByIdAsync(request.GroupId);
            
            if (group == null)
            {
                throw new NotFoundException($"Cooperative group with ID {request.GroupId} not found.");
            }

            var admin = await _unitOfWork.Repository<CooperativeAdmin>().GetByIdAsync(group.CooperativeAdminId);
            
            var allMembers = await _unitOfWork.Repository<CooperativeGroupMember>()
                .FindAsync(m => m.CooperativeGroupId == group.Id);
            
            var approvedMembers = allMembers.Where(m => m.Status == ApprovalStatus.Approved).ToList();
            var pendingMembers = allMembers.Where(m => m.Status == ApprovalStatus.Pending).ToList();
            
            var traderIds = allMembers.Select(m => m.TraderId).Distinct().ToList();
            var traders = await _unitOfWork.Repository<Trader>().FindAsync(t => traderIds.Contains(t.Id));

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

            return new CooperativeGroupDetailDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                AdminTraderId = group.CooperativeAdminId,
                AdminName = admin != null ? $"{admin.FirstName} {admin.LastName}" : "Unknown",
                MemberCount = approvedMembers.Count,
                CycleCount = cycles.Count(),
                SavingsGoal = cycles.Sum(c => c.ContributionAmount),
                TotalSaved = totalSaved,
                IsActive = isActive,
                CreatedAt = group.CreatedAt,
                Members = approvedMembers.Select(m => {
                    var trader = traders.FirstOrDefault(t => t.Id == m.TraderId);
                    return new GroupMemberSummaryDto {
                        TraderId = m.TraderId,
                        TraderName = trader != null ? $"{trader.FirstName} {trader.LastName}" : "Unknown",
                        JoinedAt = m.CreatedAt
                    };
                }).ToList(),
                PendingRequests = pendingMembers.Select(m => {
                    var trader = traders.FirstOrDefault(t => t.Id == m.TraderId);
                    return new PendingRequestDto {
                        TraderId = m.TraderId,
                        TraderName = trader != null ? $"{trader.FirstName} {trader.LastName}" : "Unknown",
                        RequestedAt = m.CreatedAt
                    };
                }).ToList(),
                ActiveCycles = cycles.Where(c => c.Status == CycleStatus.Active).Select(c => new SavingCycleSummaryDto {
                    Id = c.Id,
                    Name = c.Name,
                    ContributionAmount = c.ContributionAmount,
                    Status = c.Status.ToString()
                }).ToList(),
                CompletedCycles = cycles.Where(c => c.Status == CycleStatus.Completed).Select(c => new SavingCycleSummaryDto {
                    Id = c.Id,
                    Name = c.Name,
                    ContributionAmount = c.ContributionAmount,
                    Status = c.Status.ToString()
                }).ToList()
            };
        }
    }
}
