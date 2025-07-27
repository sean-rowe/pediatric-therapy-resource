using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;

namespace UPTRMS.Api.Services;

public interface IAuthenticationService
{
    Task<(User user, string token, string refreshToken)> RegisterAsync(RegisterRequest request);
    Task<(User user, string token, string refreshToken)> LoginAsync(LoginRequest request);
    Task<bool> ValidateTokenAsync(string token);
    Task<(string token, string refreshToken)> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string token);
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task ResetPasswordAsync(string token, string newPassword);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> VerifyEmailAsync(string token);
    Task ResendVerificationEmailAsync(string email);
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
}