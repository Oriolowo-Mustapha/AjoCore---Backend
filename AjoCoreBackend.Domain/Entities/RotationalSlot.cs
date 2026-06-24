using System;

namespace AjoCoreBackend.Domain.Entities
{
    public class RotationalSlot : BaseEntity
    {
        public Guid SavingCycleId { get; set; }
        public int SlotNumber { get; set; }
        public DateTime? EstimatedPayoutDate { get; set; }
        public bool IsAssigned { get; set; }
        public Guid? AssignedMemberId { get; set; }

        // Navigation Properties
        public SavingCycle Cycle { get; set; } = null!;
        public SavingCycleMember? AssignedMember { get; set; }
    }
}
