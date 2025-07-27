using System;
using System.Security.Claims;
using System.Threading.Tasks;
using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, string? deviceInfo = null);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    Task RevokeAllUserRefreshTokensAsync(Guid userId);
    ClaimsPrincipal? ValidateAccessToken(string token);
    string GenerateEmailVerificationToken();
    string GeneratePasswordResetToken();
}