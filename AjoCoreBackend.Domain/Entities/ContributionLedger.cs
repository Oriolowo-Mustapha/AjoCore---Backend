using System;

namespace AjoCoreBackend.Domain.Entities
{
    public class ContributionLedger : BaseEntity
    {
        public Guid SavingCycleMemberId { get; set; }
        public decimal Amount { get; set; }
        public string NombaWebhookRequestId { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; }
        
        // Navigation Properties
        public SavingCycleMember Member { get; set; } = null!;
    }
}
