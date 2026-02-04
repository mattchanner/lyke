using FluentValidation;
using Lyke.Application.DTOs.Commerce;

namespace Lyke.Application.Validators.Commerce;

public class ConversionWebhookRequestValidator : AbstractValidator<ConversionWebhookRequest>
{
    public ConversionWebhookRequestValidator()
    {
        RuleFor(x => x.ClickId)
            .NotEmpty()
            .WithMessage("ClickId is required")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("ClickId must be a valid GUID");

        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required")
            .MaximumLength(100)
            .WithMessage("OrderId must not exceed 100 characters");

        RuleFor(x => x.OrderValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("OrderValue must be non-negative");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-character code (e.g., USD, EUR)");

        RuleFor(x => x.CommissionAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("CommissionAmount must be non-negative");

        RuleFor(x => x.ProductSku)
            .MaximumLength(100)
            .When(x => x.ProductSku != null)
            .WithMessage("ProductSku must not exceed 100 characters");

        RuleFor(x => x.Signature)
            .NotEmpty()
            .WithMessage("Signature is required for webhook verification");
    }
}
