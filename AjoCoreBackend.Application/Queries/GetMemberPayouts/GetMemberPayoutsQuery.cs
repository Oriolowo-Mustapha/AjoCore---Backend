using System;
using System.Collections.Generic;
using AjoCoreBackend.Application.DTOs.Balances;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetMemberPayouts
{
    public class GetMemberPayoutsQuery : IRequest<List<PayoutLedgerDto>>
    {
        public Guid SavingCycleMemberId { get; set; }
    }
}
