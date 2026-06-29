using MediatR;
using System.Collections.Generic;

namespace AjoCoreBackend.Application.Queries.GetMyPersonalCycles
{
    public class GetMyPersonalCyclesQuery : IRequest<List<AjoCoreBackend.Application.DTOs.SavingCycleDto>>
    {
    }
}
