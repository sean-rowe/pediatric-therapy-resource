using TherapyDocs.Api.Models.DTOs;

namespace TherapyDocs.Api.Services;

public interface ILicenseVerificationService
{
    Task<LicenseVerificationResult> VerifyLicenseAsync(string licenseNumber, string state, string licenseType);
}