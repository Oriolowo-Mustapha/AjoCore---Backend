using System;
using AjoCoreBackend.Application.DTOs;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetSavingCycleById
{
    public record GetSavingCycleByIdQuery : IRequest<SavingCycleDto>
    {
        public Guid SavingCycleId { get; init; }
    }
}
