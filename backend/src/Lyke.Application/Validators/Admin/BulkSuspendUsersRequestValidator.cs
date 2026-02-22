using FluentValidation;
using Lyke.Application.DTOs.Admin;

namespace Lyke.Application.Validators.Admin;

public class BulkSuspendUsersRequestValidator : AbstractValidator<BulkSuspendUsersRequest>
{
    public BulkSuspendUsersRequestValidator()
    {
        RuleFor(x => x.UserIds)
            .NotEmpty()
            .WithMessage("At least one user ID is required");

        RuleFor(x => x.UserIds)
            .Must(ids => ids.Count <= 100)
            .When(x => x.UserIds != null)
            .WithMessage("Cannot suspend more than 100 users at once");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required");

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage("Reason must be at most 1000 characters");
    }
}
