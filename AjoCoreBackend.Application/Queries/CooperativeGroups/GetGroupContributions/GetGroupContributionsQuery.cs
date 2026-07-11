using System;
using System.Collections.Generic;
using AjoCoreBackend.Application.DTOs;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetGroupContributions
{
    public class GetGroupContributionsQuery : IRequest<List<GroupContributionLedgerDto>>
    {
        public Guid GroupId { get; set; }
        public Guid? CycleId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
