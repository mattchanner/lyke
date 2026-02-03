using FluentValidation;
using Lyke.Application.DTOs.Feed;

namespace Lyke.Application.Validators.Feed;

public class EngageRequestValidator : AbstractValidator<EngageRequest>
{
    public EngageRequestValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid engagement type");
    }
}
