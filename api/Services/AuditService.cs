using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        ApplicationDbContext context,
        ILogger<AuditService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        string action, 
        Guid? userId = null, 
        string? entityType = null, 
        Guid? entityId = null, 
        string? details = null,
        bool success = true,
        string? errorMessage = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = GetClientIpAddress(httpContext);
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            var auditLog = new AuditLog
            {
                Action = action,
                UserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                Success = success,
                ErrorMessage = errorMessage,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Don't let audit failures break the application
            _logger.LogError(ex, "Failed to write audit log for action: {Action}", action);
        }
    }

    public async Task LogAuthenticationAsync(
        string action, 
        string email, 
        bool success, 
        string? ipAddress = null, 
        string? userAgent = null,
        string? errorMessage = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            ipAddress ??= GetClientIpAddress(httpContext);
            userAgent ??= httpContext?.Request.Headers["User-Agent"].ToString();

            // Find user by email (if exists)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            var auditLog = new AuditLog
            {
                Action = $"Authentication.{action}",
                UserId = user?.UserId,
                EntityType = "User",
                EntityId = user?.UserId,
                Details = $"Email: {email}",
                Success = success,
                ErrorMessage = errorMessage,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            // Log failed attempts for security monitoring
            if (!success)
            {
                _logger.LogWarning("Failed authentication attempt for {Email} from {IpAddress}", email, ipAddress);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write authentication audit log for email: {Email}", email);
        }
    }

    public async Task LogResourceAccessAsync(
        Guid userId, 
        Guid resourceId, 
        string action, 
        string? ipAddress = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            ipAddress ??= GetClientIpAddress(httpContext);
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            var auditLog = new AuditLog
            {
                Action = $"Resource.{action}",
                UserId = userId,
                EntityType = "Resource",
                EntityId = resourceId,
                Details = $"Resource accessed: {resourceId}",
                Success = true,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write resource access audit log for resource: {ResourceId}", resourceId);
        }
    }

    public async Task LogDataAccessAsync(
        Guid userId, 
        string entityType, 
        Guid entityId, 
        string action, 
        string? ipAddress = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            ipAddress ??= GetClientIpAddress(httpContext);
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            var auditLog = new AuditLog
            {
                Action = $"DataAccess.{action}",
                UserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                Details = $"{entityType} {action}: {entityId}",
                Success = true,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            // Log sensitive data access
            if (entityType == "Student" || entityType == "User")
            {
                _logger.LogInformation("Sensitive data accessed: {EntityType} {EntityId} by user {UserId}", 
                    entityType, entityId, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write data access audit log for entity: {EntityType} {EntityId}", 
                entityType, entityId);
        }
    }

    private string? GetClientIpAddress(HttpContext? httpContext)
    {
        if (httpContext == null)
            return null;

        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Check for real IP header
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to remote IP address
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}