using FluentValidation;
using Lyke.Application.DTOs.Admin;

namespace Lyke.Application.Validators.Admin;

public class SuspendUserRequestValidator : AbstractValidator<SuspendUserRequest>
{
    public SuspendUserRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Suspension reason is required");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Suspension reason must be at most 500 characters");
    }
}
