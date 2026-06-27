using System;
using System.Collections.Generic;
using MediatR;

namespace AjoCoreBackend.Application.Commands.SavingCycles.ReorderMembers
{
    public class MemberOrderDto
    {
        public Guid MemberId { get; set; }
        public int NewOrder { get; set; }
    }

    public record ReorderCycleMembersCommand : IRequest
    {
        public Guid SavingCycleId { get; init; }
        public List<MemberOrderDto> MemberOrders { get; init; } = new List<MemberOrderDto>();
    }
}
