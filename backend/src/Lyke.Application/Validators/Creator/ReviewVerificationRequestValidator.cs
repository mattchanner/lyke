using FluentValidation;
using Lyke.Application.DTOs.Creator;

namespace Lyke.Application.Validators.Creator;

public class ReviewVerificationRequestValidator : AbstractValidator<ReviewVerificationRequest>
{
    public ReviewVerificationRequestValidator()
    {
        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .When(x => !x.Approve)
            .WithMessage("Rejection reason is required when rejecting verification");

        RuleFor(x => x.RejectionReason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.RejectionReason))
            .WithMessage("Rejection reason must be at most 500 characters");
    }
}
