using AjoCoreBackend.Application.DTOs.Balances;
using AjoCoreBackend.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Domain.Entities;
using System.Linq;

namespace AjoCoreBackend.Application.Queries.Balances.GetSystemAdminBalance
{
    public class GetSystemAdminBalanceQueryHandler : IRequestHandler<GetSystemAdminBalanceQuery, SystemAdminBalanceDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSystemAdminBalanceQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SystemAdminBalanceDto> Handle(GetSystemAdminBalanceQuery request, CancellationToken cancellationToken)
        {
            var contributions = await _unitOfWork.Ledgers.GetAllAsync();
            var payouts = await _unitOfWork.Repository<PayoutLedger>().GetAllAsync();
            var reversals = await _unitOfWork.Repository<ReversalLedger>().GetAllAsync();

            var successfulReversals = reversals.Where(r => r.Status == Domain.Enum.TransactionStatus.Success).ToList();

            var activeCycles = (await _unitOfWork.SavingCycles.FindAsync(c => c.Status == Domain.Enum.CycleStatus.Active)).ToList();
            
            var totalGroups = (await _unitOfWork.Repository<CooperativeGroup>().GetAllAsync()).Count();
            
            var totalMembers = (await _unitOfWork.Repository<CooperativeGroupMember>()
                .FindAsync(m => m.Status == Domain.Enum.ApprovalStatus.Approved)).Count();

            var totalTraders = (await _unitOfWork.Repository<Trader>().GetAllAsync()).Count();
            var totalCooperativeAdmins = (await _unitOfWork.Repository<CooperativeAdmin>().GetAllAsync()).Count();

            decimal pendingContributions = 0;
            foreach (var cycle in activeCycles)
            {
                var cycleMembers = (await _unitOfWork.SavingCycleMembers.FindAsync(m => m.SavingCycleId == cycle.Id)).ToList();
                var cycleMembersCount = cycleMembers.Count;
                var cycleTarget = cycle.CycleType == Domain.Enum.CycleType.Rosca 
                    ? cycle.ContributionAmount * cycleMembersCount 
                    : cycle.IndividualTargetAmount * cycleMembersCount;

                var cycleMemberIds = cycleMembers.Select(m => m.Id).ToList();
                var cycleTotalPaid = contributions.Where(c => cycleMemberIds.Contains(c.SavingCycleMemberId)).Sum(c => c.Amount);
                pendingContributions += System.Math.Max(0, cycleTarget - cycleTotalPaid);
            }

            return new SystemAdminBalanceDto
            {
                ActiveCycles = activeCycles.Count,
                TotalGroups = totalGroups,
                TotalMembers = totalMembers,
                PendingContributions = pendingContributions,
                Traders = totalTraders,
                CooperativeAdmins = totalCooperativeAdmins,
                TotalContributions = contributions.Sum(c => c.Amount),
                TotalPayouts = payouts.Sum(p => p.Amount),
                TotalReversals = successfulReversals.Sum(r => r.Amount)
            };
        }
    }
}
