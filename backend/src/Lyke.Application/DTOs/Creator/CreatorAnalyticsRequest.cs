namespace Lyke.Application.DTOs.Creator;

public record CreatorAnalyticsRequest(
    DateTime? StartDate,
    DateTime? EndDate
);
