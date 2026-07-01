using AjoCoreBackend.Application.DTOs.Balances;
using MediatR;

namespace AjoCoreBackend.Application.Queries.Balances.GetSystemAdminBalance
{
    public class GetSystemAdminBalanceQuery : IRequest<SystemAdminBalanceDto>
    {
    }
}
