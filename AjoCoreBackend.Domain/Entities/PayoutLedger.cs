using System;

namespace AjoCoreBackend.Domain.Entities
{
    public class PayoutLedger : BaseEntity
    {
        public Guid SavingCycleMemberId { get; set; }
        public decimal Amount { get; set; }
        public string MerchantTxRef { get; set; } = string.Empty;
        public DateTime PayoutDate { get; set; }
        
        // Navigation Properties
        public SavingCycleMember Member { get; set; } = null!;
    }
}
