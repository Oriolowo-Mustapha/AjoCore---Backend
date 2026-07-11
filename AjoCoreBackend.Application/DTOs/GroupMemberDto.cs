using System;

namespace AjoCoreBackend.Application.DTOs
{
    public record GroupMemberDto
    {
        public Guid Id { get; init; }
        public Guid TraderId { get; init; }
        public string TraderName { get; init; } = string.Empty;
        public string TraderEmail { get; init; } = string.Empty;
        public string TraderPhone { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime? ApprovedAt { get; init; }
    }
}
