using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using System;
using System.Collections.Generic;

namespace AjoCoreBackend.Domain.Entities
{
    public class SavingCycleMember : BaseEntity
    {
        public Guid SavingCycleId { get; set; }
        public Guid? NombaVirtualAccountId { get; set; }
        public Guid UserId { get; set; }
        public int PayoutOrder { get; set; }
        public MemberStatus Status { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
        
        // Navigation Properties
        public SavingCycle Cycle { get; set; } = null!;
        public NombaVirtualAccount? VirtualAccount { get; set; }
        public Trader Trader { get; set; } = null!;
        public ICollection<ContributionLedger> Contributions { get; set; } = new List<ContributionLedger>();
        public ICollection<PayoutLedger> Payouts { get; set; } = new List<PayoutLedger>();
    }
}

