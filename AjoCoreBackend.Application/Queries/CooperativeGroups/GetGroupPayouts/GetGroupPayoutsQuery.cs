using System;
using System.Collections.Generic;
using AjoCoreBackend.Application.DTOs;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetGroupPayouts
{
    public class GetGroupPayoutsQuery : IRequest<List<GroupPayoutLedgerDto>>
    {
        public Guid GroupId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
