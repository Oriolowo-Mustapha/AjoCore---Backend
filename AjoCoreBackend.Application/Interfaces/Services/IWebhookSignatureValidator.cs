using AjoCoreBackend.Application.DTOs.Nomba;

namespace AjoCoreBackend.Application.Interfaces.Services
{
    public interface IWebhookSignatureValidator
    {
        bool ValidateSignature(HookPayload payload, string signatureHeader, string timestamp);
    }
}
