using FluentValidation;
using Lyke.Application.DTOs.Creator;

namespace Lyke.Application.Validators.Creator;

public class RegisterCreatorRequestValidator : AbstractValidator<RegisterCreatorRequest>
{
    public RegisterCreatorRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Display name is required")
            .MaximumLength(100)
            .WithMessage("Display name must be at most 100 characters");

        RuleFor(x => x.Bio)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Bio))
            .WithMessage("Bio must be at most 1000 characters");
    }
}
