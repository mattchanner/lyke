namespace Lyke.Application.DTOs.Retailer;

public record ImportProductsResponse(
    int TotalRows,
    int Imported,
    int Updated,
    int Skipped,
    int Failed,
    List<ImportError> Errors
);

public record ImportError(int Row, string? ExternalSku, string ErrorMessage);
