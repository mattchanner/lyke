using Lyke.Application.DTOs.Privacy;

namespace Lyke.Application.Interfaces;

public interface IPrivacyService
{
    Task<DataExportResponse> ExportUserDataAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ConsentStatusResponse> GetConsentStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateConsentAsync(Guid userId, UpdateConsentRequest request, CancellationToken cancellationToken = default);
}
