using System;
using System.Security.Cryptography;
using System.Text;
using AjoCoreBackend.Application.Interfaces.Services;
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

        public bool ValidateSignature(string payload, string signatureHeader)
        {
            if (string.IsNullOrEmpty(signatureHeader)) return false;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_clientSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            return hashString.Equals(signatureHeader, StringComparison.OrdinalIgnoreCase);
        }
    }
}
