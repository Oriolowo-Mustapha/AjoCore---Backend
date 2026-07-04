using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Email;
using AjoCoreBackend.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AjoCoreBackend.Infrastructure.Services
{
    public class BrevoEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<BrevoEmailService> _logger;

        public BrevoEmailService(IOptions<EmailSettings> emailSettings, HttpClient httpClient, ILogger<BrevoEmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://api.brevo.com/v3/");
            _httpClient.DefaultRequestHeaders.Add("api-key", _emailSettings.Password);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var payload = new
                {
                    sender = new { name = _emailSettings.SenderName, email = _emailSettings.SenderEmail },
                    to = new[] { new { email = to } },
                    subject = subject,
                    htmlContent = isHtml ? body : null,
                    textContent = !isHtml ? body : null
                };

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("smtp/email", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send email via Brevo API to {To}. Status: {StatusCode}. Error: {Error}", to, response.StatusCode, errorResponse);
                }
                else
                {
                    _logger.LogInformation("Successfully sent email via Brevo API to {To}", to);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email via Brevo API to {To}", to);
            }
        }
    }
}
