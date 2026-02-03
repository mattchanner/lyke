using FluentValidation;
using Lyke.Application.DTOs.Profile;

namespace Lyke.Application.Validators.Profile;

public class CreateBodyProfileRequestValidator : AbstractValidator<CreateBodyProfileRequest>
{
    public CreateBodyProfileRequestValidator()
    {
        RuleFor(x => x.HeightCm)
            .InclusiveBetween(100, 250)
            .WithMessage("Height must be between 100cm and 250cm");

        RuleFor(x => x.WeightKg)
            .InclusiveBetween(30, 300)
            .WithMessage("Weight must be between 30kg and 300kg");

        RuleFor(x => x.BodyTypeId)
            .GreaterThan(0)
            .WithMessage("Body type is required");

        RuleFor(x => x.FitPreference)
            .IsInEnum()
            .When(x => x.FitPreference.HasValue)
            .WithMessage("Invalid fit preference");
    }
}
