using System;
using AjoCoreBackend.Application.DTOs;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetGroupById
{
    public record GetGroupByIdQuery : IRequest<CooperativeGroupDto>
    {
        public Guid GroupId { get; init; }
    }
}
