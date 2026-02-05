using FluentValidation;
using Lyke.Application.DTOs.Retailer;

namespace Lyke.Application.Validators.Retailer;

public class RegisterRetailerRequestValidator : AbstractValidator<RegisterRetailerRequest>
{
    public RegisterRetailerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name must be at most 200 characters");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.LogoUrl))
            .WithMessage("Logo URL must be at most 500 characters");

        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.WebsiteUrl))
            .WithMessage("Website URL must be at most 500 characters");

        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.ContactEmail))
            .WithMessage("Contact email must be a valid email address")
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.ContactEmail))
            .WithMessage("Contact email must be at most 200 characters");
    }
}
