using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Quiz;

public record QuizAnswer(int QuestionId, int AnswerIndex);

public record QuizResultRequest(List<QuizAnswer> Answers);

public record QuizResultResponse(
    int BodyTypeId,
    string BodyTypeName,
    Stature Stature,
    Build Build,
    string ResultLabel,
    string ResultDescription
);
