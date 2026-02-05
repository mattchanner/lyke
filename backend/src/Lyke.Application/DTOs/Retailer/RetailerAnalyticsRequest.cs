using Microsoft.AspNetCore.Mvc;

namespace Lyke.Application.DTOs.Retailer;

public class RetailerAnalyticsRequest
{
    [FromQuery(Name = "startDate")]
    public DateTime? StartDate { get; set; }

    [FromQuery(Name = "endDate")]
    public DateTime? EndDate { get; set; }
}
