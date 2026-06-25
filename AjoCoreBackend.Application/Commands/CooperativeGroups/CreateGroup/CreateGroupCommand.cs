using System;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.CreateGroup
{
    public record CreateGroupCommand : IRequest<Guid>
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }
}
