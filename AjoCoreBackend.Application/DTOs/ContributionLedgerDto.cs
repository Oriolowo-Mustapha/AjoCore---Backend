using System;

namespace AjoCoreBackend.Application.DTOs
{
    public record ContributionLedgerDto
    {
        public Guid Id { get; init; }
        public Guid SavingCycleMemberId { get; init; }
        public decimal Amount { get; init; }
        public DateTime PaidAt { get; init; }
    }
}
