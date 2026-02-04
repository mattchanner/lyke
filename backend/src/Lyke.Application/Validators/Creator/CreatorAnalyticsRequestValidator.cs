using FluentValidation;
using Lyke.Application.DTOs.Creator;

namespace Lyke.Application.Validators.Creator;

public class CreatorAnalyticsRequestValidator : AbstractValidator<CreatorAnalyticsRequest>
{
    public CreatorAnalyticsRequestValidator()
    {
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be greater than or equal to start date");
    }
}
