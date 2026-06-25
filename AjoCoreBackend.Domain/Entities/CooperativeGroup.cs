using System;
using System.Collections.Generic;

namespace AjoCoreBackend.Domain.Entities
{
    public class CooperativeGroup : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid AdminTraderId { get; set; }

        // Navigation Properties
        public Trader AdminTrader { get; set; } = null!;
        public ICollection<SavingCycle> Cycles { get; set; } = new List<SavingCycle>();
        public ICollection<CooperativeGroupMember> Members { get; set; } = new List<CooperativeGroupMember>();
    }
}
