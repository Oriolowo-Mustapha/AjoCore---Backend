namespace AjoCoreBackend.Application.Interfaces.Services
{
    public interface IWebhookSignatureValidator
    {
        bool ValidateSignature(string payload, string signatureHeader);
    }
}
