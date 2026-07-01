using AjoCoreBackend.Application.DTOs.Balances;
using MediatR;
using System;

namespace AjoCoreBackend.Application.Queries.Balances.GetSavingCycleBalance
{
    public class GetSavingCycleBalanceQuery : IRequest<SavingCycleBalanceDto>
    {
        public Guid SavingCycleId { get; set; }
    }
}
