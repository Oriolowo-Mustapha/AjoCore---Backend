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

            return new CooperativeAdminBalanceDto
            {
                CooperativeGroupId = request.CooperativeGroupId,
                TotalContributions = contributions.Sum(c => c.Amount),
                TotalPayouts = payouts.Sum(p => p.Amount),
                TotalReversals = reversals.Sum(r => r.Amount)
            };
        }
    }
}
