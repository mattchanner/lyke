using FluentValidation;
using Lyke.Application.DTOs.Feed;

namespace Lyke.Application.Validators.Feed;

public class FeedRequestValidator : AbstractValidator<FeedRequest>
{
    public FeedRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.SortBy).IsInEnum().WithMessage("Invalid sort option");
    }
}
