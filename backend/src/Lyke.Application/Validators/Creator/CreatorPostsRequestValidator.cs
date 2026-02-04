using FluentValidation;
using Lyke.Application.DTOs.Creator;

namespace Lyke.Application.Validators.Creator;

public class CreatorPostsRequestValidator : AbstractValidator<CreatorPostsRequest>
{
    public CreatorPostsRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid post status");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");
    }
}
