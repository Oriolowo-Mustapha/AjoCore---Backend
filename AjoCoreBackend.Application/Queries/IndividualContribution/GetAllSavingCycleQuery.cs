using AjoCoreBackend.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjoCoreBackend.Application.Queries.IndividualContribution
{
    public record GetAllSavingCycleQuery : IRequest<List<SavingCycleDto>>
    {
    }
}
