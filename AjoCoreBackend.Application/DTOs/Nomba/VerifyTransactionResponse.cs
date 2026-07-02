namespace AjoCoreBackend.Application.DTOs.Nomba
{
    public class VerifyTransactionResponse
    {
        public string Status { get; set; } = string.Empty; // "SUCCESS", "FAILED", etc.
        public decimal Amount { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public string TimeCreated { get; set; } = string.Empty;
    }
}
