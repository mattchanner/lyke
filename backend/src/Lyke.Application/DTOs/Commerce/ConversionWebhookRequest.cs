namespace Lyke.Application.DTOs.Commerce;

public record ConversionWebhookRequest(
    string ClickId,
    string OrderId,
    decimal OrderValue,
    string Currency,
    decimal CommissionAmount,
    string? ProductSku,
    DateTime TransactionDate,
    string Signature
);
