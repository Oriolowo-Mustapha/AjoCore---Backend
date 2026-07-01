using AjoCoreBackend.Application.DTOs.Balances;
using MediatR;
using System;

namespace AjoCoreBackend.Application.Queries.Balances.GetCooperativeAdminBalance
{
    public class GetCooperativeAdminBalanceQuery : IRequest<CooperativeAdminBalanceDto>
    {
        public Guid CooperativeGroupId { get; set; }
    }
}
