using FluentValidation;
using Lyke.Application.DTOs.Feed;

namespace Lyke.Application.Validators.Feed;

public class SearchRequestValidator : AbstractValidator<SearchRequest>
{
    public SearchRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Search query is required")
            .MinimumLength(2)
            .WithMessage("Search query must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("Search query must not exceed 100 characters");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue)
            .WithMessage("Invalid search type");
    }
}
