using FluentValidation;
using Lyke.Application.DTOs.Commerce;

namespace Lyke.Application.Validators.Commerce;

public class ProductSearchRequestValidator : AbstractValidator<ProductSearchRequest>
{
    public ProductSearchRequestValidator()
    {
        RuleFor(x => x.Query)
            .MinimumLength(2)
            .When(x => !string.IsNullOrEmpty(x.Query))
            .WithMessage("Query must be at least 2 characters")
            .MaximumLength(200)
            .WithMessage("Query must not exceed 200 characters");

        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.Category)
            .MaximumLength(100)
            .When(x => x.Category != null)
            .WithMessage("Category must not exceed 100 characters");
    }
}
