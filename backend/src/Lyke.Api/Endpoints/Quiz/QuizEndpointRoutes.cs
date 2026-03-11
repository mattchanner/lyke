using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Quiz;

namespace Lyke.Api.Endpoints.Quiz;

public static class QuizEndpointRoutes
{
    public static IEndpointRouteBuilder MapQuizEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quiz/v1").WithTags("Quiz");

        group
            .MapPost("/calculate", CalculateResult.Handle)
            .WithName("CalculateQuizResult")
            .WithSummary("Calculate body type quiz result")
            .WithDescription(
                "Processes quiz answers and returns the determined body type, stature, and build. Anonymous access allowed for shareability."
            )
            .AllowAnonymous()
            .Produces<ApiResponse<QuizResultResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);

        return app;
    }
}
