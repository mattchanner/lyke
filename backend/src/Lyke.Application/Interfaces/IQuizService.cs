using Lyke.Application.DTOs.Quiz;

namespace Lyke.Application.Interfaces;

public interface IQuizService
{
    Task<QuizResultResponse> CalculateResultAsync(
        QuizResultRequest request,
        CancellationToken cancellationToken = default
    );
}
