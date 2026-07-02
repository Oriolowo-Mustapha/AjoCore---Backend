using System;
using System.Collections.Generic;

namespace AjoCoreBackend.Application.DTOs
{
    public record CooperativeGroupDetailDto : CooperativeGroupDto
    {
        public List<GroupMemberSummaryDto> Members { get; init; } = new();
        public List<SavingCycleSummaryDto> ActiveCycles { get; init; } = new();
        public List<SavingCycleSummaryDto> CompletedCycles { get; init; } = new();
        public List<PendingRequestDto> PendingRequests { get; init; } = new();
    }

    public record GroupMemberSummaryDto
    {
        public Guid TraderId { get; init; }
        public string TraderName { get; init; } = string.Empty;
        public DateTime JoinedAt { get; init; }
    }

    public record SavingCycleSummaryDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal ContributionAmount { get; init; }
        public string Status { get; init; } = string.Empty;
    }

    public record PendingRequestDto
    {
        public Guid TraderId { get; init; }
        public string TraderName { get; init; } = string.Empty;
        public DateTime RequestedAt { get; init; }
    }
}
