using FluentValidation;
using Lyke.Application.DTOs.Feed;
using Lyke.Core.Enums;

namespace Lyke.Application.Validators.Feed;

public class CreateContentReportRequestValidator : AbstractValidator<CreateContentReportRequest>
{
    public CreateContentReportRequestValidator()
    {
        RuleFor(x => x.Reason)
            .IsInEnum()
            .WithMessage("Invalid report reason");

        RuleFor(x => x.AdditionalDetails)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.AdditionalDetails))
            .WithMessage("Additional details must be at most 1000 characters");

        RuleFor(x => x.AdditionalDetails)
            .NotEmpty()
            .When(x => x.Reason == ReportReason.Other)
            .WithMessage("Additional details are required when reason is Other");
    }
}
