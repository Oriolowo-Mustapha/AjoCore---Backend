using AjoCoreBackend.Application.DTOs.Balances;
using MediatR;
using System;

namespace AjoCoreBackend.Application.Queries.Balances.GetTraderBalance
{
    public class GetTraderBalanceQuery : IRequest<TraderBalanceDto>
    {
        public Guid TraderId { get; set; }
    }
}
