using AjoCoreBackend.Application.DTOs.Balances;
using AjoCoreBackend.Application.Interfaces.Repositories;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace AjoCoreBackend.Application.Queries.Balances.GetTraderBalance
{
    public class GetTraderBalanceQueryHandler : IRequestHandler<GetTraderBalanceQuery, TraderBalanceDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTraderBalanceQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TraderBalanceDto> Handle(GetTraderBalanceQuery request, CancellationToken cancellationToken)
        {
            var members = await _unitOfWork.SavingCycleMembers.FindAsync(m => m.UserId == request.TraderId);
            var memberIds = members.Select(m => m.Id).ToList();
            
            var contributions = await _unitOfWork.Ledgers.FindAsync(c => memberIds.Contains(c.SavingCycleMemberId));
            
            var cycleIds = members.Select(m => m.SavingCycleId).Distinct().ToList();
            var cycles = await _unitOfWork.SavingCycles.FindAsync(c => cycleIds.Contains(c.Id));

            var traderBalanceDto = new TraderBalanceDto
            {
                OverallTotalPaid = 0,
                PendingContributions = 0
            };

            foreach (var member in members)
            {
                var cycle = cycles.FirstOrDefault(c => c.Id == member.SavingCycleId);
                if (cycle == null) continue;

                var memberContributions = contributions.Where(c => c.SavingCycleMemberId == member.Id).ToList();
                var totalPaid = memberContributions.Sum(c => c.Amount);
                
                decimal targetAmount = cycle.IndividualTargetAmount;
                if (cycle.CycleType == Domain.Enum.CycleType.Rosca)
                {
                    // For ROSCA, target is ContributionAmount * total members
                    var allCycleMembers = await _unitOfWork.SavingCycleMembers.FindAsync(m => m.SavingCycleId == cycle.Id);
                    targetAmount = cycle.ContributionAmount * allCycleMembers.Count();
                }
                else if (cycle.CycleType == Domain.Enum.CycleType.Asca && cycle.DurationInIntervals.HasValue)
                {
                    targetAmount = cycle.ContributionAmount * cycle.DurationInIntervals.Value;
                }

                traderBalanceDto.OverallTotalPaid += totalPaid;

                if (cycle.Status == Domain.Enum.CycleStatus.Active)
                {
                    traderBalanceDto.PendingContributions += Math.Max(0, targetAmount - totalPaid);
                }

                traderBalanceDto.CycleBalances.Add(new TraderCycleBalanceDto
                {
                    CycleId = cycle.Id,
                    CycleName = cycle.Name,
                    CycleType = cycle.CycleType.ToString(),
                    CycleStatus = cycle.Status.ToString(),
                    TargetAmount = targetAmount,
                    TotalPaid = totalPaid
                });
            }

            return traderBalanceDto;
        }
    }
}
