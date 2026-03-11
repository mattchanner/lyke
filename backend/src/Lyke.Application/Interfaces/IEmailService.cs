namespace Lyke.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(
        string toAddress,
        string subject,
        string templateName,
        object templateData,
        CancellationToken cancellationToken = default
    );

    Task SendPasswordResetEmailAsync(
        string toAddress,
        string resetToken,
        CancellationToken cancellationToken = default
    );

    Task SendEmailVerificationAsync(
        string toAddress,
        string verificationToken,
        CancellationToken cancellationToken = default
    );

    Task SendWelcomeEmailAsync(
        string toAddress,
        string userType,
        CancellationToken cancellationToken = default
    );

    Task SendCreatorVerificationResultAsync(
        string toAddress,
        string creatorDisplayName,
        bool approved,
        string? rejectionReason = null,
        CancellationToken cancellationToken = default
    );

    Task SendPostModerationResultAsync(
        string toAddress,
        string postTitle,
        bool approved,
        string? rejectionReason = null,
        CancellationToken cancellationToken = default
    );

    Task SendAccountSuspensionNotificationAsync(
        string toAddress,
        string reason,
        CancellationToken cancellationToken = default
    );

    Task SendDataExportNotificationAsync(
        string toAddress,
        bool hasCreatorData,
        CancellationToken cancellationToken = default
    );
}
