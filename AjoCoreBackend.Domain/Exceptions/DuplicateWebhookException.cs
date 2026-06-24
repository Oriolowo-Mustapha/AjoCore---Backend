using System;

namespace AjoCoreBackend.Domain.Exceptions
{
    public class DuplicateWebhookException : DomainException
    {
        public string NombaWebhookRequestId { get; }

        public DuplicateWebhookException(string nombaWebhookRequestId) 
            : base("A webhook with this request ID has already been successfully processed.")
        {
            NombaWebhookRequestId = nombaWebhookRequestId;
        }
    }
}
