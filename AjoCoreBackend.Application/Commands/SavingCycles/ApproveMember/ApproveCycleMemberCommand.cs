using System;
using MediatR;

namespace AjoCoreBackend.Application.Commands.SavingCycles.ApproveMember
{
    public record ApproveCycleMemberCommand : IRequest
    {
        public Guid SavingCycleId { get; init; }
        public Guid MemberId { get; init; }
    }
}
