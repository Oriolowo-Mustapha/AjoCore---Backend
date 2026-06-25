using System;
using MediatR;

namespace AjoCoreBackend.Application.Commands.StartSavingCycle
{
    public record StartSavingCycleCommand : IRequest<bool>
    {
        public Guid SavingCycleId { get; init; }
    }
}
