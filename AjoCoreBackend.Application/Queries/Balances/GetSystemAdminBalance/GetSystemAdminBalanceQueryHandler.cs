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

            return new SystemAdminBalanceDto
            {
                TotalContributions = contributions.Sum(c => c.Amount),
                TotalPayouts = payouts.Sum(p => p.Amount),
                TotalReversals = successfulReversals.Sum(r => r.Amount)
            };
        }
    }
}
