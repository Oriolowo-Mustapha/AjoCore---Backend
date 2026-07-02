namespace AjoCoreBackend.Application.DTOs.Nomba
{
    public record NombaApiResponse<T>
    {
        [System.Text.Json.Serialization.JsonPropertyName("code")]
        public string Code { get; init; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public T? Data { get; init; }
    }

    // Sub-Account Models
    public record CreateSubAccountRequest
    {
        public string AccountName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }

    public record CreateSubAccountResponse
    {
        public string SubAccountId { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
    }

    // Virtual Account Models
    public record CreateVirtualAccountRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("accountRef")]
        public string AccountReference { get; init; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("accountName")]
        public string AccountName { get; init; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("currency")]
        public string Currency { get; init; } = "NGN";

        [System.Text.Json.Serialization.JsonPropertyName("bvn")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
        public string? Bvn { get; init; }
    }

    public record CreateVirtualAccountResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("bankAccountNumber")]
        public string AccountNumber { get; init; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("bankName")]
        public string BankName { get; init; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("bankAccountName")]
        public string AccountName { get; init; } = string.Empty;
    }

    // Transfer Lookup Models
    public record BankLookupRequest
    {
        public string AccountNumber { get; init; } = string.Empty;
        public string BankCode { get; init; } = string.Empty;
    }

    public record BankLookupResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("accountName")]
        public string AccountName { get; init; } = string.Empty;
    }

    // Transfer Bank Models
    public record BankTransferRequest
    {
        public decimal Amount { get; init; } // Note: Nomba expects Kobo, we will transform this before HTTP send or serialize correctly
        public string AccountNumber { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public string BankCode { get; init; } = string.Empty;
        public string MerchantTxRef { get; init; } = string.Empty;
        public string SenderName { get; init; } = string.Empty;
    }

    public record BankTransferResponse
    {
        public string Status { get; init; } = string.Empty; // e.g., "SUCCESS", "PENDING"
        public string Message { get; init; } = string.Empty;
        public string TransactionId { get; init; } = string.Empty;
    }

    // Bank Codes Models
    public record FetchBanksResponse
    {
        public System.Collections.Generic.List<BankDto> Data { get; init; } = new();
    }

    public record BankDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("code")]
        public string BankCode { get; init; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string BankName { get; init; } = string.Empty;
    }
}
