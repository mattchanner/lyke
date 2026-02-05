using FluentValidation;
using Lyke.Application.DTOs.Retailer;

namespace Lyke.Application.Validators.Retailer;

public class UpdateRetailerProductRequestValidator : AbstractValidator<UpdateRetailerProductRequest>
{
    public UpdateRetailerProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name must be at most 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .When(x => x.Price.HasValue)
            .WithMessage("Price must be greater than 0");

        RuleFor(x => x.Currency)
            .Length(3)
            .When(x => !string.IsNullOrEmpty(x.Currency))
            .WithMessage("Currency must be a 3-character code");

        RuleFor(x => x.ProductUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.ProductUrl))
            .WithMessage("Product URL must be at most 500 characters");

        RuleFor(x => x.Category)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Category))
            .WithMessage("Category must be at most 100 characters");
    }
}
