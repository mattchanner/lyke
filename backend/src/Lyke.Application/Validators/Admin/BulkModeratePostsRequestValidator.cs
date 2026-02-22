using FluentValidation;
using Lyke.Application.DTOs.Admin;

namespace Lyke.Application.Validators.Admin;

public class BulkModeratePostsRequestValidator : AbstractValidator<BulkModeratePostsRequest>
{
    public BulkModeratePostsRequestValidator()
    {
        RuleFor(x => x.PostIds)
            .NotEmpty()
            .WithMessage("At least one post ID is required");

        RuleFor(x => x.PostIds)
            .Must(ids => ids.Count <= 100)
            .When(x => x.PostIds != null)
            .WithMessage("Cannot moderate more than 100 posts at once");

        RuleFor(x => x.Action)
            .IsInEnum()
            .WithMessage("Invalid bulk action");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .When(x => x.Action == BulkPostAction.Reject || x.Action == BulkPostAction.Remove)
            .WithMessage("Reason is required for Reject and Remove actions");

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage("Reason must be at most 1000 characters");
    }
}
