using System;
using System.Collections.Generic;
using AjoCoreBackend.Application.DTOs;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetGroupMembers
{
    public record GetGroupMembersQuery : IRequest<List<GroupMemberDto>>
    {
        public Guid GroupId { get; init; }
    }
}
