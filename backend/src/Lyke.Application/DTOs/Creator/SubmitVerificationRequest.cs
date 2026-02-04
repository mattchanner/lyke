namespace Lyke.Application.DTOs.Creator;

public record SubmitVerificationRequest(
    List<string> DocumentUrls,
    string? Notes
);
