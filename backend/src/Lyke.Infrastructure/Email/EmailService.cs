using System.Collections.Concurrent;
using System.Reflection;
using Azure;
using Azure.Communication.Email;
using Fluid;
using Lyke.Application.Configuration;
using Lyke.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly EmailClient? _emailClient;
    private readonly ILogger<EmailService> _logger;
    private readonly FluidParser _parser;

    private readonly ConcurrentDictionary<string, IFluidTemplate> _templateCache = new();
    private readonly ConcurrentDictionary<string, List<DateTime>> _rateLimitTracker = new();

    public EmailService(
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _parser = new FluidParser();

        if (!_settings.DryRun && !string.IsNullOrEmpty(_settings.ConnectionString))
        {
            _emailClient = new EmailClient(_settings.ConnectionString);
        }
    }

    public async Task SendEmailAsync(
        string toAddress,
        string subject,
        string templateName,
        object templateData,
        CancellationToken cancellationToken = default)
    {
        if (IsRateLimited(toAddress))
        {
            _logger.LogWarning("Email rate limit exceeded for {Email}", toAddress);
            return;
        }

        var template = GetOrLoadTemplate(templateName);
        var context = new TemplateContext(templateData);
        var htmlBody = await template.RenderAsync(context);

        RecordSend(toAddress);

        if (_settings.DryRun || _emailClient == null)
        {
            _logger.LogInformation(
                "[DRY RUN] Email to={To}, subject={Subject}, template={Template}, body length={Length}",
                toAddress, subject, templateName, htmlBody.Length);
            return;
        }

        try
        {
            var emailMessage = new EmailMessage(
                senderAddress: _settings.SenderAddress,
                content: new EmailContent(subject) { Html = htmlBody },
                recipients: new EmailRecipients(
                    [new EmailAddress(toAddress)]));

            var operation = await _emailClient.SendAsync(
                WaitUntil.Started, emailMessage, cancellationToken);

            _logger.LogInformation(
                "Email sent to {To}, subject={Subject}, operationId={OperationId}",
                toAddress, subject, operation.Id);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex,
                "Failed to send email to {To}, subject={Subject}, ACS error={ErrorCode}",
                toAddress, subject, ex.ErrorCode);
        }
    }

    public Task SendPasswordResetEmailAsync(
        string toAddress, string resetToken, CancellationToken cancellationToken = default)
    {
        var baseUrl = string.IsNullOrEmpty(_settings.ApiBaseUrl)
            ? _settings.AppBaseUrl
            : _settings.ApiBaseUrl;

        return SendEmailAsync(toAddress, "Reset Your Password", "password-reset", new
        {
            api_base_url = baseUrl,
            reset_token = Uri.EscapeDataString(resetToken),
            email = Uri.EscapeDataString(toAddress)
        }, cancellationToken);
    }

    public Task SendEmailVerificationAsync(
        string toAddress, string verificationToken, CancellationToken cancellationToken = default)
    {
        var baseUrl = string.IsNullOrEmpty(_settings.ApiBaseUrl)
            ? _settings.AppBaseUrl
            : _settings.ApiBaseUrl;

        return SendEmailAsync(toAddress, "Verify Your Email", "email-verification", new
        {
            api_base_url = baseUrl,
            verification_token = Uri.EscapeDataString(verificationToken),
            email = Uri.EscapeDataString(toAddress)
        }, cancellationToken);
    }

    public Task SendWelcomeEmailAsync(
        string toAddress, string userType, CancellationToken cancellationToken = default)
    {
        return SendEmailAsync(toAddress, "Welcome to LYKE!", "welcome", new
        {
            app_base_url = _settings.AppBaseUrl,
            user_type = userType
        }, cancellationToken);
    }

    public Task SendCreatorVerificationResultAsync(
        string toAddress, string creatorDisplayName, bool approved,
        string? rejectionReason = null, CancellationToken cancellationToken = default)
    {
        var subject = approved ? "Creator Verification Approved" : "Creator Verification Update";
        return SendEmailAsync(toAddress, subject, "creator-verification-result", new
        {
            creator_display_name = creatorDisplayName,
            approved,
            rejection_reason = rejectionReason ?? string.Empty
        }, cancellationToken);
    }

    public Task SendPostModerationResultAsync(
        string toAddress, string postTitle, bool approved,
        string? rejectionReason = null, CancellationToken cancellationToken = default)
    {
        var subject = approved ? "Your Post Has Been Published" : "Post Review Update";
        return SendEmailAsync(toAddress, subject, "post-moderation-result", new
        {
            post_title = postTitle,
            approved,
            rejection_reason = rejectionReason ?? string.Empty
        }, cancellationToken);
    }

    public Task SendAccountSuspensionNotificationAsync(
        string toAddress, string reason, CancellationToken cancellationToken = default)
    {
        return SendEmailAsync(toAddress, "Account Suspended", "account-suspension", new
        {
            reason
        }, cancellationToken);
    }

    private IFluidTemplate GetOrLoadTemplate(string templateName)
    {
        return _templateCache.GetOrAdd(templateName, name =>
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Lyke.Infrastructure.Email.Templates.{name}.liquid";

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Email template '{name}' not found as embedded resource '{resourceName}'");

            using var reader = new StreamReader(stream);
            var templateSource = reader.ReadToEnd();

            if (!_parser.TryParse(templateSource, out var template, out var error))
            {
                throw new InvalidOperationException(
                    $"Failed to parse Liquid template '{name}': {error}");
            }

            return template;
        });
    }

    private bool IsRateLimited(string emailAddress)
    {
        var key = emailAddress.ToLowerInvariant();
        var cutoff = DateTime.UtcNow - TimeSpan.FromMinutes(_settings.RateLimitWindowMinutes);

        if (_rateLimitTracker.TryGetValue(key, out var timestamps))
        {
            lock (timestamps)
            {
                timestamps.RemoveAll(t => t < cutoff);
                return timestamps.Count >= _settings.RateLimitMaxPerWindow;
            }
        }

        return false;
    }

    private void RecordSend(string emailAddress)
    {
        var key = emailAddress.ToLowerInvariant();
        var timestamps = _rateLimitTracker.GetOrAdd(key, _ => new List<DateTime>());

        lock (timestamps)
        {
            timestamps.Add(DateTime.UtcNow);
        }
    }
}
