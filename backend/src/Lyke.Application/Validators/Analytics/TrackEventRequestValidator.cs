using FluentValidation;
using Lyke.Application.DTOs.Analytics;

namespace Lyke.Application.Validators.Analytics;

public class TrackEventRequestValidator : AbstractValidator<TrackEventRequest>
{
    public TrackEventRequestValidator()
    {
        RuleFor(x => x.EventType)
            .IsInEnum()
            .WithMessage("Invalid event type");

        RuleFor(x => x.EntityType)
            .MaximumLength(50)
            .When(x => x.EntityType != null)
            .WithMessage("EntityType must not exceed 50 characters");

        RuleFor(x => x.SessionId)
            .MaximumLength(100)
            .When(x => x.SessionId != null)
            .WithMessage("SessionId must not exceed 100 characters");
    }
}

public class TrackEventBatchRequestValidator : AbstractValidator<TrackEventBatchRequest>
{
    public TrackEventBatchRequestValidator()
    {
        RuleFor(x => x.Events)
            .NotEmpty()
            .WithMessage("Events list cannot be empty");

        RuleFor(x => x.Events.Count)
            .LessThanOrEqualTo(100)
            .When(x => x.Events != null)
            .WithMessage("Batch cannot exceed 100 events");

        RuleForEach(x => x.Events)
            .SetValidator(new TrackEventRequestValidator());
    }
}
