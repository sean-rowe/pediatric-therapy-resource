using TherapyDocs.Api.Models.DTOs;

namespace TherapyDocs.Api.Interfaces;

public interface IUserRegistrationService
{
    Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string? ipAddress, string? userAgent);
}