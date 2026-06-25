using System;
using MediatR;

namespace AjoCoreBackend.Application.Commands.RecordContribution
{
    public record RecordContributionCommand : IRequest<Guid>
    {
        public string WebhookRequestId { get; init; } = string.Empty;
        public string AccountNumber { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string TransactionReference { get; init; } = string.Empty;
    }
}
