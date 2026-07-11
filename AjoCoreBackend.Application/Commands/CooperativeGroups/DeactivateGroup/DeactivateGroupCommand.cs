using System;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.DeactivateGroup
{
    public class DeactivateGroupCommand : IRequest<bool>
    {
        public Guid GroupId { get; set; }
    }
}
