namespace Lyke.Application.DTOs.Privacy;

public record UpdateConsentRequest(bool? MarketingOptIn = null, bool? AcceptPrivacyPolicy = null);
