using FluentValidation;
using Lyke.Application.DTOs.Creator;

namespace Lyke.Application.Validators.Creator;

public class PostProductRequestValidator : AbstractValidator<PostProductRequest>
{
    public PostProductRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.SizeWorn)
            .NotEmpty()
            .WithMessage("Size worn is required")
            .MaximumLength(50)
            .WithMessage("Size worn must be at most 50 characters");

        RuleFor(x => x.FitRating)
            .IsInEnum()
            .When(x => x.FitRating.HasValue)
            .WithMessage("Invalid fit rating");

        RuleFor(x => x.FitNotes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.FitNotes))
            .WithMessage("Fit notes must be at most 500 characters");

        RuleFor(x => x.StylingNotes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.StylingNotes))
            .WithMessage("Styling notes must be at most 500 characters");
    }
}
