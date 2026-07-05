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
            var timestampHeader = Request.Headers["nomba-timestamp"].ToString();
            
            if (string.IsNullOrEmpty(timestampHeader))
            {
                timestampHeader = Request.Headers["x-nomba-timestamp"].ToString();
            }
            
            using var reader = new StreamReader(Request.Body);
            var payloadStr = await reader.ReadToEndAsync();

            AjoCoreBackend.Application.DTOs.Nomba.HookPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<AjoCoreBackend.Application.DTOs.Nomba.HookPayload>(payloadStr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize Nomba webhook payload: {Payload}", payloadStr);
                return Ok(); // Prevent retries
            }

            if (payload == null)
            {
                return Ok();
            }

            var timestamp = !string.IsNullOrEmpty(timestampHeader) ? timestampHeader : (payload.Data.Transaction?.Time ?? "");

            if (!_signatureValidator.ValidateSignature(payload, signatureHeader, timestamp))
            {
                _logger.LogWarning("Invalid webhook signature received from Nomba. Payload: {Payload}, Signature: {Signature}", payloadStr, signatureHeader);
                return Unauthorized("Invalid webhook signature.");
            }

            try
            {
                var webhookRequestId = payload.RequestId ?? Guid.NewGuid().ToString();
                var accountNumber = payload.Data.Transaction?.AliasAccountNumber ?? "";
                var amount = payload.Data.Transaction?.TransactionAmount ?? 0m;
                var txRef = payload.Data.Transaction?.TransactionId ?? "";

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
