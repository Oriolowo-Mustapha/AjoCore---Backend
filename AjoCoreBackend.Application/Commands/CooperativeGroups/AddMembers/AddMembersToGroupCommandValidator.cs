using FluentValidation;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.AddMembers
{
    public class AddMembersToGroupCommandValidator : AbstractValidator<AddMembersToGroupCommand>
    {
        public AddMembersToGroupCommandValidator()
        {
            RuleFor(x => x.GroupId).NotEmpty().WithMessage("GroupId is required.");
            RuleFor(x => x.Members).NotEmpty().WithMessage("At least one member is required.");
            RuleForEach(x => x.Members).SetValidator(new AddGroupMemberDtoValidator());
        }
    }

    public class AddGroupMemberDtoValidator : AbstractValidator<AddGroupMemberDto>
    {
        public AddGroupMemberDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Valid email is required.");
                
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.");
                
            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of Birth is required.")
                .Must(BeAtLeast18YearsOld).WithMessage("Trader must be at least 18 years old.");
        }

        private bool BeAtLeast18YearsOld(System.DateTime dateOfBirth)
        {
            var today = System.DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age >= 18;
        }
    }
}
