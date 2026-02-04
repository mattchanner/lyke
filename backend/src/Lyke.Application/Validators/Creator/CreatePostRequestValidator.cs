using FluentValidation;
using Lyke.Application.DTOs.Creator;

namespace Lyke.Application.Validators.Creator;

public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Title must be at most 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must be at most 2000 characters");

        RuleFor(x => x.MediaType)
            .IsInEnum()
            .WithMessage("Invalid media type");

        RuleFor(x => x.MediaUrls)
            .NotEmpty()
            .WithMessage("At least one media URL is required");

        RuleFor(x => x.Products)
            .NotEmpty()
            .WithMessage("At least one product is required");

        RuleForEach(x => x.Products)
            .SetValidator(new PostProductRequestValidator());
    }
}
