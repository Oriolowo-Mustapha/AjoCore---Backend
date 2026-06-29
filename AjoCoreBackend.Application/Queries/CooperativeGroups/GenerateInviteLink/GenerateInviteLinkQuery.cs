using System;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GenerateInviteLink
{
    public class GenerateInviteLinkQuery : IRequest<string>
    {
        public Guid GroupId { get; set; }
        public string BaseUrl { get; set; } = string.Empty;
    }
}
