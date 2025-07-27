using System;

namespace UPTRMS.Api.Models.Domain;

public class AuditLog
{
    public Guid AuditId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Details { get; set; } // JSON data
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Navigation property
    public virtual User? User { get; set; }
}