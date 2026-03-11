using FluentValidation;
using Lyke.Application.DTOs.Auth;

namespace Lyke.Application.Validators.Auth;

public class SocialLoginRequestValidator : AbstractValidator<SocialLoginRequest>
{
    private static readonly string[] SupportedProviders = ["Google", "Apple"];

    public SocialLoginRequestValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("Provider is required")
            .Must(p => SupportedProviders.Contains(p))
            .WithMessage("Provider must be 'Google' or 'Apple'");

        RuleFor(x => x.IdToken).NotEmpty().WithMessage("ID token is required");
    }
}
