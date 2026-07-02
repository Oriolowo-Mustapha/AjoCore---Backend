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

            return new SystemAdminBalanceDto
            {
                ActiveCycles = activeCycles.Count,
                TotalGroups = totalGroups,
                TotalMembers = totalMembers,
                PendingContributions = pendingContributions,
                TotalContributions = contributions.Sum(c => c.Amount),
                TotalPayouts = payouts.Sum(p => p.Amount),
                TotalReversals = successfulReversals.Sum(r => r.Amount)
            };
        }
    }
}
