using System;
using System.Collections.Generic;
using AjoCoreBackend.Domain.Enum;

namespace AjoCoreBackend.Domain.Entities
{
    public class SavingCycleMember : BaseEntity
    {
        public Guid SavingCycleId { get; set; }
        public Guid NombaVirtualAccountId { get; set; }
        public Guid UserId { get; set; }
        public int PayoutOrder { get; set; }
        public MemberStatus Status { get; set; }
        
        // Navigation Properties
        public SavingCycle Cycle { get; set; } = null!;
        public NombaVirtualAccount VirtualAccount { get; set; } = null!;
        public ICollection<ContributionLedger> Contributions { get; set; } = new List<ContributionLedger>();
        public ICollection<PayoutLedger> Payouts { get; set; } = new List<PayoutLedger>();
    }
}
