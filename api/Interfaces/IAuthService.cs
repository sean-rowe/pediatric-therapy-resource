using System.Threading.Tasks;
using TherapyDocs.Api.Models.DTOs;

namespace TherapyDocs.Api.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string? ipAddress, string? userAgent);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> ResendVerificationEmailAsync(string email);
}