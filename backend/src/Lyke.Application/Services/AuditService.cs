using System.Text.Json;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Application.Services;

public class AuditService : IAuditService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<AuditService> _logger;
    private readonly PrivacySettings _settings;

    public AuditService(
        DbContext dbContext,
        ILogger<AuditService> logger,
        IOptions<PrivacySettings> settings)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task LogAsync(
        Guid userId,
        AuditAction action,
        Guid? targetUserId = null,
        string? entityType = null,
        Guid? entityId = null,
        object? details = null,
        string? ipAddress = null)
    {
        if (!_settings.Audit.Enabled)
            return;

        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TargetUserId = targetUserId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details != null ? JsonSerializer.Serialize(details) : null,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            _dbContext.Set<AuditLog>().Add(auditLog);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log for action {Action} by user {UserId}", action, userId);
        }
    }

    public async Task<(IReadOnlyList<AuditLogResponse> Logs, PaginationMeta Meta)> GetAuditLogsAsync(
        AuditLogQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<AuditLog>().AsNoTracking().AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        if (request.TargetUserId.HasValue)
            query = query.Where(a => a.TargetUserId == request.TargetUserId.Value);

        if (request.Action.HasValue)
            query = query.Where(a => a.Action == request.Action.Value);

        if (request.From.HasValue)
            query = query.Where(a => a.Timestamp >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(a => a.Timestamp <= request.To.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogResponse(
                a.Id,
                a.UserId,
                a.TargetUserId,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.Details,
                a.IpAddress,
                a.Timestamp))
            .ToListAsync(cancellationToken);

        var meta = new PaginationMeta
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return (logs, meta);
    }
}
