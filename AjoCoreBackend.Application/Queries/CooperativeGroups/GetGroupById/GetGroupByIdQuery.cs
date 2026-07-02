using System;
using AjoCoreBackend.Application.DTOs;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetGroupById
{
    public record GetGroupByIdQuery : IRequest<CooperativeGroupDetailDto>
    {
        public Guid GroupId { get; init; }
    }
}
