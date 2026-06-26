using FluentValidation;

namespace AjoCoreBackend.Application.Commands.Auth.VerifyEmail
{
    public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        public VerifyEmailCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email is required.");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token is required.");
        }
    }
}
