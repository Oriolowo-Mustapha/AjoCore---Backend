using System;
using System.Collections.Generic;
using AjoCoreBackend.Application.DTOs;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetPendingRequests
{
    public record GetPendingRequestsQuery : IRequest<List<GroupMemberDto>>
    {
        public Guid GroupId { get; init; }
    }
}
