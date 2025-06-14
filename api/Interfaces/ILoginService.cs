using TherapyDocs.Api.Models.DTOs;

namespace TherapyDocs.Api.Interfaces;

public interface ILoginService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent);
}