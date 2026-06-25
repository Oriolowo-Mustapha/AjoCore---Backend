using System;
using System.Collections.Generic;
using AjoCoreBackend.Application.DTOs;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetMemberContributions
{
    public record GetMemberContributionsQuery : IRequest<List<ContributionLedgerDto>>
    {
        public Guid SavingCycleMemberId { get; init; }
    }
}
