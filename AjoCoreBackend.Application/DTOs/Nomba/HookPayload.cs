using System.Text.Json.Serialization;

namespace AjoCoreBackend.Application.DTOs.Nomba
{
    public class HookPayload
    {
        [JsonPropertyName("event_type")]
        public string? EventType { get; set; }

        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("data")]
        public HookData Data { get; set; } = new();
    }

    public class HookData
    {
        [JsonPropertyName("merchant")]
        public Merchant? Merchant { get; set; }

        [JsonPropertyName("transaction")]
        public Transaction? Transaction { get; set; }

        [JsonPropertyName("customer")]
        public Customer? Customer { get; set; }

        [JsonPropertyName("terminal")]
        public object? Terminal { get; set; }
    }

    public class Merchant
    {
        [JsonPropertyName("walletId")]
        public string? WalletId { get; set; }

        [JsonPropertyName("walletBalance")]
        public decimal? WalletBalance { get; set; }

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }
    }

    public class Transaction
    {
        [JsonPropertyName("transactionId")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("responseCode")]
        public string? ResponseCode { get; set; }

        [JsonPropertyName("aliasAccountNumber")]
        public string? AliasAccountNumber { get; set; }

        [JsonPropertyName("transactionAmount")]
        public decimal? TransactionAmount { get; set; }
    }

    public class Customer
    {
        [JsonPropertyName("bankCode")]
        public string? BankCode { get; set; }

        [JsonPropertyName("senderName")]
        public string? SenderName { get; set; }

        [JsonPropertyName("bankName")]
        public string? BankName { get; set; }

        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; set; }
    }
}
