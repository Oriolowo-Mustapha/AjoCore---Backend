using System;
using System.Collections.Generic;
using AjoCoreBackend.Domain.Enum;

using System.ComponentModel.DataAnnotations.Schema;

namespace AjoCoreBackend.Domain.Entities
{
    public class SavingCycle : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public CycleType CycleType { get; set; }
        public decimal ContributionAmount { get; set; }
        public int IntervalDays { get; set; }
        
        [NotMapped]
        public decimal IndividualTargetAmount =>
        EndDate == null || StartDate == null
         ? 0
         : ContributionAmount *
           (((EndDate.Value - StartDate.Value).Days / IntervalDays) + 1);

        public int? DurationInIntervals { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public CycleStatus Status { get; set; }
        public Guid? CooperativeGroupId { get; set; }
        
        // Navigation Properties
        public CooperativeGroup? CooperativeGroup { get; set; }
        public ICollection<SavingCycleMember> Members { get; set; } = new List<SavingCycleMember>();
}
}
