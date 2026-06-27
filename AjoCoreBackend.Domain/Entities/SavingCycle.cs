using System;
using System.Collections.Generic;
using AjoCoreBackend.Domain.Enum;

namespace AjoCoreBackend.Domain.Entities
{
    public class SavingCycle : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public CycleType CycleType { get; set; }
        public decimal ContributionAmount { get; set; }
        public int IntervalDays { get; set; }
        public string NombaSubAccountId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public CycleStatus Status { get; set; }

        public Guid? IndividualOwnerId { get; set; }
        public Guid? CooperativeGroupId { get; set; }
        
        // Navigation Properties
        public CooperativeGroup? CooperativeGroup { get; set; }
        public ICollection<SavingCycleMember> Members { get; set; } = new List<SavingCycleMember>();

        public SavingCycleMember? IndividualOwner { get; set; }
    }
}
