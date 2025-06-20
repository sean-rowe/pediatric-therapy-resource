using TherapyDocs.Api.Models;

namespace TherapyDocs.Api.Interfaces;

public interface IRegistrationAuditRepository
{
    Task LogRegistrationAttemptAsync(string email, string? licenseNumber, string? licenseState, 
        bool success, string? failureReason, string? ipAddress, string? userAgent);
}