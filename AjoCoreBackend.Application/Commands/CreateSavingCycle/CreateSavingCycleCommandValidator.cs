using AjoCoreBackend.Domain.Enum;
using FluentValidation;

namespace AjoCoreBackend.Application.Commands.CreateSavingCycle
{
    public class CreateSavingCycleCommandValidator : AbstractValidator<CreateSavingCycleCommand>
    {
        public CreateSavingCycleCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Cycle name is required.")
                .MaximumLength(100).WithMessage("Cycle name must not exceed 100 characters.");

            RuleFor(x => x.CycleType)
                .NotEmpty().WithMessage("Cycle type is required.")
                .Must(value => System.Enum.TryParse<CycleType>(value, ignoreCase: true, out _))
                .WithMessage("Cycle type must be either 'Rosca' or 'Asca'.");

            RuleFor(x => x.ContributionAmount)
                .GreaterThan(0).WithMessage("Contribution amount must be greater than zero.");

            RuleFor(x => x.IntervalDays)
                .GreaterThan(0).WithMessage("Interval days must be greater than zero.");
        }
    }
}
