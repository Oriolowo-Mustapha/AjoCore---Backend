using System.Collections.Generic;
using AjoCoreBackend.Application.DTOs;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetAllGroups
{
    public record GetAllGroupsQuery : IRequest<List<CooperativeGroupDto>>
    {
        public string? SearchTerm { get; init; }
    }
}
