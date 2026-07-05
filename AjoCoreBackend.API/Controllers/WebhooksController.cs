using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Commands.RecordContribution;
using AjoCoreBackend.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AjoCoreBackend.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    public class WebhooksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWebhookSignatureValidator _signatureValidator;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(IMediator mediator, IWebhookSignatureValidator signatureValidator, ILogger<WebhooksController> logger)
        {
            _mediator = mediator;
            _signatureValidator = signatureValidator;
            _logger = logger;
        }

        [HttpPost("nomba")]
        public async Task<IActionResult> NombaWebhook()
        {
            var signatureHeader = Request.Headers["nomba-signature"].ToString();
            
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            if (!_signatureValidator.ValidateSignature(payload, signatureHeader))
            {
                _logger.LogWarning("Invalid webhook signature received from Nomba. Payload: {Payload}, Signature: {Signature}", payload, signatureHeader);
                return Unauthorized("Invalid webhook signature.");
            }

            try
            {
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;
                
                var data = root.GetProperty("data");
                var transaction = data.GetProperty("transaction");

                var webhookRequestId = root.TryGetProperty("requestId", out var reqIdProp) ? reqIdProp.GetString() : Guid.NewGuid().ToString();
                var accountNumber = transaction.GetProperty("aliasAccountNumber").GetString() ?? "";
                var amount = transaction.GetProperty("transactionAmount").GetDecimal(); // Naira
                var txRef = transaction.GetProperty("transactionId").GetString() ?? "";

                var command = new RecordContributionCommand
                {
                    WebhookRequestId = webhookRequestId,
                    AccountNumber = accountNumber,
                    Amount = amount,
                    TransactionReference = txRef
                };

                await _mediator.Send(command);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse or process Nomba webhook payload: {Payload}", payload);
                // We return Ok() even on parsing failure to prevent infinite Nomba retries
                return Ok(); 
            }
        }
    }
}
