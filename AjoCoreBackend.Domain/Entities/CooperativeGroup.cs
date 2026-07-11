using System;
using System.Collections.Generic;
using AjoCoreBackend.Domain.Enum;

namespace AjoCoreBackend.Domain.Entities
{
    public class CooperativeGroup : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid CooperativeAdminId { get; set; }
        public GroupStatus Status { get; set; } = GroupStatus.Active;

        // Navigation Properties
        public CooperativeAdmin CooperativeAdmin { get; set; } = null!;
        public ICollection<SavingCycle> Cycles { get; set; } = new List<SavingCycle>();
        public ICollection<CooperativeGroupMember> Members { get; set; } = new List<CooperativeGroupMember>();
    }
}
