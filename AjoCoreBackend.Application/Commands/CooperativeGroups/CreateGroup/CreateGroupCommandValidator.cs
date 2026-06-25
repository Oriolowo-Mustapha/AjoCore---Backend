using FluentValidation;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.CreateGroup
{
    public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
    {
        public CreateGroupCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Group name is required.")
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Group description is required.")
                .MaximumLength(1000);
        }
    }
}
