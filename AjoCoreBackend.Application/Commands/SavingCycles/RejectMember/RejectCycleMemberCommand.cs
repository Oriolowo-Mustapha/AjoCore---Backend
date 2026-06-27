using System;
using MediatR;

namespace AjoCoreBackend.Application.Commands.SavingCycles.RejectMember
{
    public record RejectCycleMemberCommand : IRequest
    {
        public Guid SavingCycleId { get; init; }
        public Guid MemberId { get; init; }
    }
}
