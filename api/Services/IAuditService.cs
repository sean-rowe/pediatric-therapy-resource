using System;
using System.Threading.Tasks;

namespace UPTRMS.Api.Services;

public interface IAuditService
{
    Task LogAsync(
        string action, 
        Guid? userId = null, 
        string? entityType = null, 
        Guid? entityId = null, 
        string? details = null,
        bool success = true,
        string? errorMessage = null);
        
    Task LogAuthenticationAsync(
        string action, 
        string email, 
        bool success, 
        string? ipAddress = null, 
        string? userAgent = null,
        string? errorMessage = null);
        
    Task LogResourceAccessAsync(
        Guid userId, 
        Guid resourceId, 
        string action, 
        string? ipAddress = null);
        
    Task LogDataAccessAsync(
        Guid userId, 
        string entityType, 
        Guid entityId, 
        string action, 
        string? ipAddress = null);
}