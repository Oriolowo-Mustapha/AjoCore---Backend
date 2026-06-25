using FluentValidation;

namespace AjoCoreBackend.Application.Commands.JoinSavingCycle
{
    public class JoinSavingCycleCommandValidator : AbstractValidator<JoinSavingCycleCommand>
    {
        public JoinSavingCycleCommandValidator()
        {
            RuleFor(x => x.SavingCycleId)
                .NotEmpty().WithMessage("SavingCycleId is required.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");
        }
    }
}
