using FluentValidation;

namespace AjoCoreBackend.Application.Commands.Auth.UpdateBvn
{
    public class UpdateBvnCommandValidator : AbstractValidator<UpdateBvnCommand>
    {
        public UpdateBvnCommandValidator()
        {
            RuleFor(x => x.Bvn)
                .NotEmpty().WithMessage("BVN is required.")
                .Length(11).WithMessage("BVN must be exactly 11 digits.")
                .Matches("^[0-9]+$").WithMessage("BVN must contain only numbers.");
        }
    }
}
