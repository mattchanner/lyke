using FluentValidation;
using Lyke.Application.DTOs.Creator;

namespace Lyke.Application.Validators.Creator;

public class SubmitVerificationRequestValidator : AbstractValidator<SubmitVerificationRequest>
{
    public SubmitVerificationRequestValidator()
    {
        RuleFor(x => x.DocumentUrls)
            .NotEmpty()
            .WithMessage("At least one verification document is required");

        RuleFor(x => x.DocumentUrls)
            .Must(urls => urls.Count <= 5)
            .When(x => x.DocumentUrls != null)
            .WithMessage("Maximum 5 documents can be submitted");

        RuleForEach(x => x.DocumentUrls)
            .NotEmpty()
            .WithMessage("Document URL cannot be empty")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Document URL must be a valid URL");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must be at most 1000 characters");
    }
}
