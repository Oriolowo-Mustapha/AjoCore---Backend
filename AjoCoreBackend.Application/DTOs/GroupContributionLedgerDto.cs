using System;

namespace AjoCoreBackend.Application.DTOs
{
    public record GroupContributionLedgerDto
    {
        public Guid Id { get; init; }
        public DateTime PaidAt { get; init; }
        public decimal Amount { get; init; }
        public string MemberName { get; init; } = string.Empty;
        public string CycleName { get; init; } = string.Empty;
        public string WebhookId { get; init; } = string.Empty;
    }
}
