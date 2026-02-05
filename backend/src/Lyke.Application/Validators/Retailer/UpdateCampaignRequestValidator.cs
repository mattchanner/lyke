using FluentValidation;
using Lyke.Application.DTOs.Retailer;

namespace Lyke.Application.Validators.Retailer;

public class UpdateCampaignRequestValidator : AbstractValidator<UpdateCampaignRequest>
{
    public UpdateCampaignRequestValidator()
    {
        RuleFor(x => x.BudgetAmount)
            .GreaterThan(0)
            .When(x => x.BudgetAmount.HasValue)
            .WithMessage("Budget amount must be greater than 0");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be after start date");
    }
}
