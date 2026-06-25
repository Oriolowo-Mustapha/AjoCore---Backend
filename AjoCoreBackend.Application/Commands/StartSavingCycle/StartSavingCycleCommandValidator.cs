using FluentValidation;

namespace AjoCoreBackend.Application.Commands.StartSavingCycle
{
    public class StartSavingCycleCommandValidator : AbstractValidator<StartSavingCycleCommand>
    {
        public StartSavingCycleCommandValidator()
        {
            RuleFor(x => x.SavingCycleId)
                .NotEmpty().WithMessage("SavingCycleId is required.");
        }
    }
}
