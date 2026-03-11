using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Quiz;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Quiz;

public static class CalculateResult
{
    public static async Task<IResult> Handle(
        QuizResultRequest request,
        IQuizService quizService,
        CancellationToken cancellationToken
    )
    {
        if (request.Answers == null || request.Answers.Count == 0)
        {
            return Results.BadRequest(
                ApiResponse.Fail("INVALID_REQUEST", "Quiz answers are required")
            );
        }

        var result = await quizService.CalculateResultAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<QuizResultResponse>.Ok(result));
    }
}
