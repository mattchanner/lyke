using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Style;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Style;

public static class ScoreKibbeQuiz
{
    public static IResult Handle(
        [FromBody] KibbeScoreRequest request,
        IKibbeQuizService kibbeQuizService
    )
    {
        var result = kibbeQuizService.Score(request);
        return Results.Ok(ApiResponse<KibbeScoreResponse>.Ok(result));
    }
}
