using AjoCoreBackend.Application.DTOs.Balances;
using AjoCoreBackend.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using AjoCoreBackend.Domain.Entities;

namespace AjoCoreBackend.Application.Queries.Balances.GetSavingCycleBalance
{
    public class GetSavingCycleBalanceQueryHandler : IRequestHandler<GetSavingCycleBalanceQuery, SavingCycleBalanceDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSavingCycleBalanceQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SavingCycleBalanceDto> Handle(GetSavingCycleBalanceQuery request, CancellationToken cancellationToken)
        {
            var contributions = await _unitOfWork.Ledgers.FindAsync(cl => cl.Member.SavingCycleId == request.SavingCycleId);
            
            var payoutsRepo = _unitOfWork.Repository<PayoutLedger>();
            var payouts = await payoutsRepo.FindAsync(pl => pl.Member.SavingCycleId == request.SavingCycleId);

            var reversalsRepo = _unitOfWork.Repository<ReversalLedger>();
            var reversals = await reversalsRepo.FindAsync(rl => rl.Status == Domain.Enum.TransactionStatus.Success && rl.Member.SavingCycleId == request.SavingCycleId);

            return new SavingCycleBalanceDto
            {
                SavingCycleId = request.SavingCycleId,
                TotalContributions = contributions.Sum(c => c.Amount),
                TotalPayouts = payouts.Sum(p => p.Amount),
                TotalReversals = reversals.Sum(r => r.Amount)
            };
        }
    }
}
