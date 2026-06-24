using System;

namespace AjoCoreBackend.Application.DTOs
{
    public record PayoutLedgerDto
    {
        public Guid Id { get; init; }
        public Guid SavingCycleMemberId { get; init; }
        public decimal Amount { get; init; }
        public DateTime PayoutDate { get; init; }
        public string MerchantTxRef { get; init; } = string.Empty;
    }
}
