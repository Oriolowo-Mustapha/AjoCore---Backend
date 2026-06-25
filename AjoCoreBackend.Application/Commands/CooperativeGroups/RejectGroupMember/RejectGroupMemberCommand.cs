using System;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.RejectGroupMember
{
    public record RejectGroupMemberCommand : IRequest<bool>
    {
        public Guid MembershipId { get; init; }
    }
}
