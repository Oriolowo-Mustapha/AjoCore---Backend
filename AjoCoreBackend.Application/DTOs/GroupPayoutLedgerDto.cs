using System;

namespace AjoCoreBackend.Application.DTOs
{
    public record GroupPayoutLedgerDto
    {
        public Guid Id { get; init; }
        public DateTime PayoutDate { get; init; }
        public decimal Amount { get; init; }
        public string MerchantTxRef { get; init; } = string.Empty;
        public string MemberName { get; init; } = string.Empty;
        public string CycleName { get; init; } = string.Empty;
    }
}
