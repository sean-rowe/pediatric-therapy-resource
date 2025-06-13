using TherapyDocs.Api.Models.DTOs;

namespace TherapyDocs.Api.Services;

public interface IAuthService
{
    Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string? ipAddress, string? userAgent);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> ResendVerificationEmailAsync(string email);
}