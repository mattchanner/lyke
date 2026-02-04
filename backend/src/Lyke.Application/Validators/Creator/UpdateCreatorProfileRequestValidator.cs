using FluentValidation;
using Lyke.Application.DTOs.Creator;

namespace Lyke.Application.Validators.Creator;

public class UpdateCreatorProfileRequestValidator : AbstractValidator<UpdateCreatorProfileRequest>
{
    public UpdateCreatorProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.DisplayName))
            .WithMessage("Display name must be at most 100 characters");

        RuleFor(x => x.Bio)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Bio))
            .WithMessage("Bio must be at most 1000 characters");
    }
}
