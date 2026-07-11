using System;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.RemoveMember
{
    public class RemoveMemberCommand : IRequest<bool>
    {
        public Guid GroupId { get; set; }
        public Guid MembershipId { get; set; }
    }
}
