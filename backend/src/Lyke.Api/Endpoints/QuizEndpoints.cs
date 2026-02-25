using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Quiz;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints;

public static class QuizEndpoints
{
    public static IEndpointRouteBuilder MapQuizEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quiz/v1").WithTags("Quiz");

        group
            .MapPost("/calculate", CalculateResultAsync)
            .WithName("CalculateQuizResult")
            .WithSummary("Calculate body type quiz result")
            .WithDescription("Processes quiz answers and returns the determined body type, stature, and build. Anonymous access allowed for shareability.")
            .AllowAnonymous()
            .Produces<ApiResponse<QuizResultResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> CalculateResultAsync(
        QuizResultRequest request,
        IQuizService quizService,
        CancellationToken cancellationToken)
    {
        if (request.Answers == null || request.Answers.Count == 0)
        {
            return Results.BadRequest(ApiResponse.Fail("INVALID_REQUEST", "Quiz answers are required"));
        }

        var result = await quizService.CalculateResultAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<QuizResultResponse>.Ok(result));
    }
}
