using AjoCoreBackend.Application.DTOs.IndividualSavingCycle;
using System;
using System.Collections.Generic;

namespace AjoCoreBackend.Application.DTOs
{
    public record SavingCycleDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string CycleType { get; init; } = string.Empty;
        public decimal ContributionAmount { get; init; }
        public int IntervalDays { get; init; }
        public string Status { get; init; } = string.Empty;
        public Guid? CooperativeGroupId { get; init; }
        public decimal TargetAmount { get; init; }
        public int? DurationInIntervals { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public DateTime CreatedAt { get; init; }
        public decimal TotalSaved { get; init; }
        public List<SavingCycleMemberDto>? Members { get; init; } = new List<SavingCycleMemberDto>();
        public IndividualSavingCycleDto? IndividualSavingCycle { get; init; }
    }
}
