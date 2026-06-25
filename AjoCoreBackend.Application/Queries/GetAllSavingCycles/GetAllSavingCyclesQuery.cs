using System.Collections.Generic;
using AjoCoreBackend.Application.DTOs;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetAllSavingCycles
{
    public record GetAllSavingCyclesQuery : IRequest<List<SavingCycleDto>>
    {
    }
}
