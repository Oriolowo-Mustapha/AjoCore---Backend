using System;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.ApproveGroupMember
{
    public record ApproveGroupMemberCommand : IRequest<bool>
    {
        public Guid MembershipId { get; init; }
    }
}
