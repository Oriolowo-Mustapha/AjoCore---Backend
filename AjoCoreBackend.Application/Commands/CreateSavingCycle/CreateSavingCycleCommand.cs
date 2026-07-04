using System;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CreateSavingCycle
{
    public record CreateSavingCycleCommand : IRequest<Guid>
    {
        public string Name { get; init; } = string.Empty;
        public string CycleType { get; init; } = string.Empty;
        public decimal ContributionAmount { get; init; }
        public int IntervalDays { get; init; }
        public int? DurationInIntervals { get; init; }
        public Guid? CooperativeGroupId { get; init; }
    }
}
