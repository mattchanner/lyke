using FluentValidation;
using Lyke.Application.DTOs.Admin;

namespace Lyke.Application.Validators.Admin;

public class ModeratePostRequestValidator : AbstractValidator<ModeratePostRequest>
{
    public ModeratePostRequestValidator()
    {
        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .When(x => !x.Approve)
            .WithMessage("Rejection reason is required when rejecting a post");

        RuleFor(x => x.RejectionReason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.RejectionReason))
            .WithMessage("Rejection reason must be at most 1000 characters");
    }
}
