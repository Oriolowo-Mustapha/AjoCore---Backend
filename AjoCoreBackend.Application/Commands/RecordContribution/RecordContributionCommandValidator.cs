using FluentValidation;

namespace AjoCoreBackend.Application.Commands.RecordContribution
{
    public class RecordContributionCommandValidator : AbstractValidator<RecordContributionCommand>
    {
        public RecordContributionCommandValidator()
        {
            RuleFor(x => x.WebhookRequestId)
                .NotEmpty().WithMessage("WebhookRequestId is required.");

            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("AccountNumber is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");
        }
    }
}
