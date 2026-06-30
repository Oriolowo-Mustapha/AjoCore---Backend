using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Commands.RecordContribution;
using AjoCoreBackend.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AjoCoreBackend.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    public class WebhooksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWebhookSignatureValidator _signatureValidator;

        public WebhooksController(IMediator mediator, IWebhookSignatureValidator signatureValidator)
        {
            _mediator = mediator;
            _signatureValidator = signatureValidator;
        }

        [HttpPost("nomba")]
        public async Task<IActionResult> NombaWebhook()
        {
            var signatureHeader = Request.Headers["nomba-signature"].ToString();
            
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            // TEMPORARILY DISABLED FOR OUTRAY TESTING
            // if (!_signatureValidator.ValidateSignature(payload, signatureHeader))
            // {
            //     return Unauthorized("Invalid webhook signature.");
            // }

            try
            {
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;
                
                var data = root.GetProperty("data");
                var webhookRequestId = root.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();
                var accountNumber = data.GetProperty("accountNumber").GetString() ?? "";
                var amount = data.GetProperty("amount").GetDecimal(); // Kobo
                var txRef = data.GetProperty("transactionReference").GetString() ?? "";

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
            catch (Exception)
            {
                // We return Ok() even on parsing failure to prevent infinite Nomba retries
                return Ok(); 
            }
        }
    }
}
