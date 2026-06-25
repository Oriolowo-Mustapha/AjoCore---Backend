using System;
using MediatR;

namespace AjoCoreBackend.Application.Commands.JoinSavingCycle
{
    public record JoinSavingCycleCommand : IRequest<Guid>
    {
        public Guid SavingCycleId { get; init; }
    }
}
