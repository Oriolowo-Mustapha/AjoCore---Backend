using System;

namespace AjoCoreBackend.Application.DTOs.Balances
{
    public class PayoutLedgerDto
    {
        public Guid Id { get; set; }
        public Guid SavingCycleMemberId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PayoutDate { get; set; }
        public string MerchantTxRef { get; set; } = string.Empty;
    }
}
