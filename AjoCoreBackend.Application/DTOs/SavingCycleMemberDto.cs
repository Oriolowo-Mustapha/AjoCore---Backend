using System;

namespace AjoCoreBackend.Application.DTOs
{
    public record SavingCycleMemberDto
    {
        public Guid Id { get; init; }
        public Guid SavingCycleId { get; init; }
        public string? VirtualAccountNumber { get; init; }
        public string? VirtualAccountBank { get; init; }
        public int PayoutOrder { get; init; }
        public string Status { get; set; } = string.Empty;
        public string ApprovalStatus { get; init; } = string.Empty;
        public DateTime JoinedAt { get; init; }
        public Guid UserId { get; init; }
        public string TraderName { get; init; } = string.Empty;
        public string TraderEmail { get; init; } = string.Empty;
        public decimal TotalContributed { get; init; }
    }
}
