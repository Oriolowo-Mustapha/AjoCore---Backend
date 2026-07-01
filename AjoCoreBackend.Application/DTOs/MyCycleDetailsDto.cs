using System;

namespace AjoCoreBackend.Application.DTOs
{
    public record MyCycleDetailsDto
    {
        public Guid CycleId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string CycleType { get; init; } = string.Empty;
        public decimal ContributionAmount { get; init; }
        public int IntervalDays { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public string? VirtualAccountNumber { get; init; }
        public string? VirtualAccountBank { get; init; }
        public int PayoutOrder { get; init; }
    }
}
