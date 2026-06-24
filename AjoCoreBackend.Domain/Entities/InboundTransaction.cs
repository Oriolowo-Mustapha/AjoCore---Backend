using System;
using AjoCoreBackend.Domain.Enum;

namespace AjoCoreBackend.Domain.Entities
{
    public class InboundTransaction : BaseEntity
    {
        public Guid TraderId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;

        // Navigation Property
        public Trader Trader { get; set; } = null!;
    }
}
