namespace Lyke.Application.DTOs.Privacy;

public record ConsentStatusResponse(
    bool MarketingOptIn,
    string? PrivacyPolicyVersion,
    DateTime? PrivacyPolicyAcceptedAt,
    string CurrentPolicyVersion,
    bool NeedsReconsent
);
