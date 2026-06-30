using AjoCoreBackend.Domain.Enum;

namespace AjoCoreBackend.Domain.Entities
{
    public class ReversalLedger : BaseEntity
    {
        public Guid SavingCycleMemberId { get; set; }
        public decimal Amount { get; set; }
        public string OriginalWebhookRequestId { get; set; } = string.Empty;
        public string ReversalTxRef { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public string? FailureReason { get; set; }
        public DateTime TriggeredAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        public SavingCycleMember Member { get; set; } = null!;
    }
}