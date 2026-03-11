using FluentValidation;
using Lyke.Application.DTOs.Commerce;

namespace Lyke.Application.Validators.Commerce;

public class TrackClickRequestValidator : AbstractValidator<TrackClickRequest>
{
    private static readonly string[] ValidSources =
    [
        "feed",
        "post_detail",
        "search",
        "similar_posts",
    ];
    private static readonly string[] ValidPlatforms = ["ios", "android", "web"];

    public TrackClickRequestValidator()
    {
        RuleFor(x => x.PostId).NotEmpty().WithMessage("PostId is required");

        RuleFor(x => x.PostProductId).NotEmpty().WithMessage("PostProductId is required");

        RuleFor(x => x.SessionId)
            .MaximumLength(100)
            .When(x => x.SessionId != null)
            .WithMessage("SessionId must not exceed 100 characters");

        RuleFor(x => x.Source)
            .Must(s => s == null || ValidSources.Contains(s.ToLower()))
            .WithMessage($"Source must be one of: {string.Join(", ", ValidSources)}");

        RuleFor(x => x.Platform)
            .Must(p => p == null || ValidPlatforms.Contains(p.ToLower()))
            .WithMessage($"Platform must be one of: {string.Join(", ", ValidPlatforms)}");

        RuleFor(x => x.FeedPosition)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FeedPosition.HasValue)
            .WithMessage("FeedPosition must be non-negative");

        RuleFor(x => x.SearchQuery)
            .MaximumLength(200)
            .When(x => x.SearchQuery != null)
            .WithMessage("SearchQuery must not exceed 200 characters");

        RuleFor(x => x.AppVersion)
            .MaximumLength(20)
            .When(x => x.AppVersion != null)
            .WithMessage("AppVersion must not exceed 20 characters");
    }
}
