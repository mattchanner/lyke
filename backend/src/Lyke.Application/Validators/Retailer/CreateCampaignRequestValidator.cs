using FluentValidation;
using Lyke.Application.DTOs.Retailer;

namespace Lyke.Application.Validators.Retailer;

public class CreateCampaignRequestValidator : AbstractValidator<CreateCampaignRequest>
{
    public CreateCampaignRequestValidator()
    {
        RuleFor(x => x.BudgetAmount)
            .GreaterThan(0)
            .WithMessage("Budget amount must be greater than 0");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Start date must be today or in the future");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");
    }
}
