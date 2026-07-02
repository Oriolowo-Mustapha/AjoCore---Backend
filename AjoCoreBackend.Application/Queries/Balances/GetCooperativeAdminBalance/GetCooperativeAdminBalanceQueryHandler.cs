using AjoCoreBackend.Application.DTOs.Balances;
using AjoCoreBackend.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using AjoCoreBackend.Domain.Entities;

namespace AjoCoreBackend.Application.Queries.Balances.GetCooperativeAdminBalance
{
    public class GetCooperativeAdminBalanceQueryHandler : IRequestHandler<GetCooperativeAdminBalanceQuery, CooperativeAdminBalanceDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCooperativeAdminBalanceQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CooperativeAdminBalanceDto> Handle(GetCooperativeAdminBalanceQuery request, CancellationToken cancellationToken)
        {
            var allCycles = await _unitOfWork.SavingCycles.FindAsync(c => c.CooperativeGroupId == request.CooperativeGroupId);
            var cycleIds = allCycles.Select(c => c.Id).ToList();

            var contributions = await _unitOfWork.Ledgers.FindAsync(cl => cycleIds.Contains(cl.Member.SavingCycleId));
            
            var payoutsRepo = _unitOfWork.Repository<PayoutLedger>();
            var payouts = await payoutsRepo.FindAsync(pl => cycleIds.Contains(pl.Member.SavingCycleId));

            var reversalsRepo = _unitOfWork.Repository<ReversalLedger>();
            var reversals = await reversalsRepo.FindAsync(rl => rl.Status == Domain.Enum.TransactionStatus.Success && cycleIds.Contains(rl.Member.SavingCycleId));

            var activeCycles = allCycles.Where(c => c.Status == Domain.Enum.CycleStatus.Active).ToList();
            var activeCycleIds = activeCycles.Select(c => c.Id).ToList();

            var membersRepo = _unitOfWork.Repository<CooperativeGroupMember>();
            var totalMembers = (await membersRepo.FindAsync(m => m.CooperativeGroupId == request.CooperativeGroupId && m.Status == Domain.Enum.ApprovalStatus.Approved)).Count();

            var groupRepo = _unitOfWork.Repository<CooperativeGroup>();
            var group = await groupRepo.GetByIdAsync(request.CooperativeGroupId);
            var totalGroups = group != null ? (await groupRepo.FindAsync(g => g.CooperativeAdminId == group.CooperativeAdminId)).Count() : 0;

            decimal pendingContributions = 0;
            foreach (var cycle in activeCycles)
            {
                var cycleMembersCount = (await _unitOfWork.SavingCycleMembers.FindAsync(m => m.SavingCycleId == cycle.Id)).Count();
                var cycleTarget = cycle.CycleType == Domain.Enum.CycleType.Rosca 
                    ? cycle.ContributionAmount * cycleMembersCount 
                    : cycle.IndividualTargetAmount * cycleMembersCount;

                var cycleTotalPaid = contributions.Where(c => c.Member.SavingCycleId == cycle.Id).Sum(c => c.Amount);
                pendingContributions += System.Math.Max(0, cycleTarget - cycleTotalPaid);
            }

            return new CooperativeAdminBalanceDto
            {
                CooperativeGroupId = request.CooperativeGroupId,
                ActiveCycles = activeCycles.Count,
                TotalMembers = totalMembers,
                TotalGroups = totalGroups,
                PendingContributions = pendingContributions,
                TotalContributions = contributions.Sum(c => c.Amount),
                TotalPayouts = payouts.Sum(p => p.Amount),
                TotalReversals = reversals.Sum(r => r.Amount)
            };
        }
    }
}
