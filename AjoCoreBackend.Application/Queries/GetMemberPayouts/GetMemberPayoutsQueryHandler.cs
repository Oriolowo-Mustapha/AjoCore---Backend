using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Balances;
using AjoCoreBackend.Application.Interfaces.Repositories;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetMemberPayouts
{
    public class GetMemberPayoutsQueryHandler : IRequestHandler<GetMemberPayoutsQuery, List<PayoutLedgerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMemberPayoutsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<PayoutLedgerDto>> Handle(GetMemberPayoutsQuery request, CancellationToken cancellationToken)
        {
            var payouts = await _unitOfWork.Repository<AjoCoreBackend.Domain.Entities.PayoutLedger>()
                .FindAsync(p => p.SavingCycleMemberId == request.SavingCycleMemberId);

            return payouts.OrderByDescending(p => p.PayoutDate)
                .Select(p => new PayoutLedgerDto
                {
                    Id = p.Id,
                    SavingCycleMemberId = p.SavingCycleMemberId,
                    Amount = p.Amount,
                    PayoutDate = p.PayoutDate,
                    MerchantTxRef = p.MerchantTxRef
                }).ToList();
        }
    }
}
