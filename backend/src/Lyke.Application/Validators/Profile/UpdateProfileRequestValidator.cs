using FluentValidation;
using Lyke.Application.DTOs.Profile;

namespace Lyke.Application.Validators.Profile;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");

        RuleFor(x => x.Email)
            .MaximumLength(256)
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email must not exceed 256 characters");
    }
}
