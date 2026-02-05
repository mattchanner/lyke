using FluentValidation;
using Lyke.Application.DTOs.Retailer;

namespace Lyke.Application.Validators.Retailer;

public class RetailerAnalyticsRequestValidator : AbstractValidator<RetailerAnalyticsRequest>
{
    public RetailerAnalyticsRequestValidator()
    {
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be greater than or equal to start date");
    }
}
