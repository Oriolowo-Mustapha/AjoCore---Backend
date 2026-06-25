using System;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.RequestJoinGroup
{
    public record RequestJoinGroupCommand : IRequest<Guid>
    {
        public Guid GroupId { get; init; }
    }
}
