using FluentValidation;
using Lyke.Application.DTOs.Privacy;

namespace Lyke.Application.Validators.Privacy;

public class UpdateConsentRequestValidator : AbstractValidator<UpdateConsentRequest>
{
    public UpdateConsentRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.MarketingOptIn.HasValue || x.AcceptPrivacyPolicy.HasValue)
            .WithMessage("At least one consent field must be provided");
    }
}
