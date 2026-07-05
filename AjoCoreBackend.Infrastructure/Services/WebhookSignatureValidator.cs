using System;
using System.Security.Cryptography;
using System.Text;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Application.DTOs.Nomba;
using Microsoft.Extensions.Configuration;

namespace AjoCoreBackend.Infrastructure.Services
{
    public class WebhookSignatureValidator : IWebhookSignatureValidator
    {
        private readonly string _clientSecret;

        public WebhookSignatureValidator(IConfiguration configuration)
        {
            _clientSecret = configuration["Nomba:WebhookSigningKey"] 
                ?? throw new ArgumentNullException("Nomba WebhookSigningKey is missing in configuration.");
        }

        public bool ValidateSignature(HookPayload payload, string signatureHeader, string timestamp)
        {
            if (string.IsNullOrEmpty(signatureHeader) || payload == null) return false;

            var data = payload.Data;
            var merchant = data?.Merchant;
            var transaction = data?.Transaction;

            string eventType = payload.EventType ?? "";
            string requestId = payload.RequestId ?? "";
            string userId = merchant?.UserId ?? "";
            string walletId = merchant?.WalletId ?? "";
            string transactionId = transaction?.TransactionId ?? "";
            string transactionType = transaction?.Type ?? "";
            string transactionTime = transaction?.Time ?? "";
            string transactionResponseCode = transaction?.ResponseCode ?? "";

            if (transactionResponseCode == "null")
            {
                transactionResponseCode = "";
            }

            var hashingPayload = $"{eventType}:{requestId}:{userId}:{walletId}:{transactionId}:{transactionType}:{transactionTime}:{transactionResponseCode}:{timestamp}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_clientSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(hashingPayload));
            var expectedSignature = Convert.ToBase64String(hash);

            return string.Equals(expectedSignature, signatureHeader, StringComparison.OrdinalIgnoreCase);
        }
    }
}
