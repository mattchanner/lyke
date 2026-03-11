using FluentValidation;
using Lyke.Application.DTOs.Quiz;

namespace Lyke.Application.Validators.Quiz;

public class QuizResultRequestValidator : AbstractValidator<QuizResultRequest>
{
    private const int MinQuestions = 1;
    private const int MaxQuestions = 10;
    private const int MaxQuestionId = 10;
    private const int MaxAnswerIndex = 10;

    public QuizResultRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotNull()
            .WithMessage("Answers are required")
            .NotEmpty()
            .WithMessage("At least one answer is required")
            .Must(a => a == null || a.Count <= MaxQuestions)
            .WithMessage($"Cannot exceed {MaxQuestions} answers");

        RuleForEach(x => x.Answers)
            .ChildRules(answer =>
            {
                answer
                    .RuleFor(a => a.QuestionId)
                    .InclusiveBetween(1, MaxQuestionId)
                    .WithMessage($"Question ID must be between 1 and {MaxQuestionId}");

                answer
                    .RuleFor(a => a.AnswerIndex)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Answer index cannot be negative")
                    .LessThanOrEqualTo(MaxAnswerIndex)
                    .WithMessage($"Answer index cannot exceed {MaxAnswerIndex}");
            });
    }
}
