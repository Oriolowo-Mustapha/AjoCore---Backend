using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjoCoreBackend.Application.Commands.IndividualContriution.CreateIndividualSavingCycle
{
    public record CreateIndividualSavingCycleCommand:IRequest<Guid>
    {
        public string Name { get; init; } = string.Empty;
        public string CycleType { get; init; } = string.Empty;
        public decimal ContributionAmount { get; init; }
        public int IntervalDays { get; init; }
        public Guid? CooperativeGroupId { get; init; }
    }
}
