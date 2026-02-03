using FluentValidation;
using Lyke.Application.DTOs.Profile;

namespace Lyke.Application.Validators.Profile;

public class UpdateBodyProfileRequestValidator : AbstractValidator<UpdateBodyProfileRequest>
{
    public UpdateBodyProfileRequestValidator()
    {
        RuleFor(x => x.HeightCm)
            .InclusiveBetween(100, 250)
            .When(x => x.HeightCm.HasValue)
            .WithMessage("Height must be between 100cm and 250cm");

        RuleFor(x => x.WeightKg)
            .InclusiveBetween(30, 300)
            .When(x => x.WeightKg.HasValue)
            .WithMessage("Weight must be between 30kg and 300kg");

        RuleFor(x => x.BodyTypeId)
            .GreaterThan(0)
            .When(x => x.BodyTypeId.HasValue)
            .WithMessage("Invalid body type");

        RuleFor(x => x.FitPreference)
            .IsInEnum()
            .When(x => x.FitPreference.HasValue)
            .WithMessage("Invalid fit preference");
    }
}
