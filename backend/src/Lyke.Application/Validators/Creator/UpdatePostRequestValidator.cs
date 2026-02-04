using FluentValidation;
using Lyke.Application.DTOs.Creator;

namespace Lyke.Application.Validators.Creator;

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
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
            .When(x => x.MediaType.HasValue)
            .WithMessage("Invalid media type");

        RuleForEach(x => x.Products)
            .SetValidator(new PostProductRequestValidator())
            .When(x => x.Products != null && x.Products.Any());
    }
}
