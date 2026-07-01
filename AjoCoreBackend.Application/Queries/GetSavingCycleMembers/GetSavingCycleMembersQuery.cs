using AjoCoreBackend.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace AjoCoreBackend.Application.Queries.GetSavingCycleMembers
{
    public class GetSavingCycleMembersQuery : IRequest<List<SavingCycleMemberDto>>
    {
        public Guid SavingCycleId { get; set; }
    }
}
