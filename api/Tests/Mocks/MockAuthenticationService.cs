using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Services;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace UPTRMS.Api.Tests.Mocks;

public class MockAuthenticationService : IAuthenticationService
{
    // User storage moved to SharedMockUserStore
    private static readonly Dictionary<string, string> _refreshTokens = new();
    private static readonly Dictionary<string, string> _emailVerificationTokens = new();

    // Static constructor removed - SharedMockUserStore handles initialization

    public static void ClearState()
    {
        _refreshTokens.Clear();
        _emailVerificationTokens.Clear();
        SharedMockUserStore.Clear();
    }

    // Seed data is now managed in SharedMockUserStore

    public async Task<(User user, string token, string refreshToken)> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        if (SharedMockUserStore.UserExistsByEmail(request.Email))
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            LicenseNumber = request.LicenseNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Languages = request.Languages ?? new List<string> { "English" },
            Specialties = request.Specialties ?? new List<string>(),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailVerified = false,
            TwoFactorEnabled = false,
            PreferredLanguage = "en",
            SubscriptionTier = SubscriptionTier.Free,
            SubscriptionStatus = SubscriptionStatus.Active
        };

        SharedMockUserStore.AddUser(user);

        // Generate tokens
        var token = GenerateToken(user);
        var refreshToken = Guid.NewGuid().ToString();
        _refreshTokens[refreshToken] = user.Email;

        // Generate email verification token
        var emailToken = Guid.NewGuid().ToString();
        _emailVerificationTokens[emailToken] = user.Email;

        return await Task.FromResult((user, token, refreshToken));
    }

    public async Task<(User user, string token, string refreshToken)> LoginAsync(LoginRequest request)
    {
        var user = SharedMockUserStore.GetUserByEmail(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.EmailVerified)
        {
            throw new UnauthorizedAccessException("Email not verified");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is not active");
        }

        var token = GenerateToken(user);
        var refreshToken = Guid.NewGuid().ToString();
        _refreshTokens[refreshToken] = user.Email;

        user.LastLoginAt = DateTime.UtcNow;

        return await Task.FromResult((user, token, refreshToken));
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        // For testing, always verify successfully if token is "test-token"
        if (token == "test-token")
        {
            // Find the most recently created user and verify them
            var lastUser = SharedMockUserStore.GetAllUsers().OrderByDescending(u => u.CreatedAt).FirstOrDefault();
            if (lastUser != null)
            {
                lastUser.EmailVerified = true;
                return await Task.FromResult(true);
            }
        }

        if (_emailVerificationTokens.TryGetValue(token, out var email))
        {
            var user = SharedMockUserStore.GetUserByEmail(email);
            if (user != null)
            {
                user.EmailVerified = true;
                _emailVerificationTokens.Remove(token);
                return await Task.FromResult(true);
            }
        }

        return await Task.FromResult(false);
    }

    public async Task<bool> VerifyEmailByEmailAsync(string email)
    {
        var user = SharedMockUserStore.GetUserByEmail(email);
            if (user != null)
        {
            user.EmailVerified = true;
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email)
    {
        return await Task.FromResult(SharedMockUserStore.UserExistsByEmail(email));
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        // For testing purposes, always succeed
        await Task.CompletedTask;
    }

    public async Task<(string token, string refreshToken)> RefreshTokenAsync(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var email))
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var user = SharedMockUserStore.GetUserByEmail(email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        var newToken = GenerateToken(user);
        var newRefreshToken = Guid.NewGuid().ToString();
        
        _refreshTokens.Remove(refreshToken);
        _refreshTokens[newRefreshToken] = email;

        return await Task.FromResult((newToken, newRefreshToken));
    }

    public async Task LogoutAsync(string token)
    {
        // For testing purposes, we'll parse the mock token to get the user ID
        // Mock tokens are in format "mock-token-{userId}-{ticks}"
        if (token.StartsWith("mock-token-"))
        {
            var parts = token.Split('-');
            if (parts.Length >= 4 && Guid.TryParse(parts[2], out var userId))
            {
                var userEmail = SharedMockUserStore.GetUserById(userId)?.Email;
                if (userEmail != null)
                {
                    var tokensToRemove = _refreshTokens.Where(kvp => kvp.Value == userEmail).Select(kvp => kvp.Key).ToList();
                    foreach (var refreshToken in tokensToRemove)
                    {
                        _refreshTokens.Remove(refreshToken);
                    }
                }
            }
        }
        await Task.CompletedTask;
    }

    private string GenerateToken(User user)
    {
        // Generate a properly signed JWT token for testing
        var key = System.Text.Encoding.ASCII.GetBytes("super-secret-key-that-is-at-least-32-characters-long-for-testing-purposes");
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString()),
                new System.Security.Claims.Claim("user_id", user.UserId.ToString()),
                new System.Security.Claims.Claim("email", user.Email),
                new System.Security.Claims.Claim("subscription_tier", user.SubscriptionTier.ToString()),
                new System.Security.Claims.Claim("organization_id", user.OrganizationId?.ToString() ?? ""),
                new System.Security.Claims.Claim("is_seller", user.IsSellerApproved.ToString().ToLower())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "TherapyDocsTest",
            Audience = "TherapyDocsTest",
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        // For testing purposes, always validate successfully
        return await Task.FromResult(true);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        if (!SharedMockUserStore.UserExistsByEmail(email))
        {
            throw new InvalidOperationException("User not found");
        }
        
        // Generate a simple reset token
        var resetToken = $"reset-{Guid.NewGuid()}";
        return await Task.FromResult(resetToken);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = SharedMockUserStore.GetUserById(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await Task.CompletedTask;
    }

    public async Task ResendVerificationEmailAsync(string email)
    {
        if (!SharedMockUserStore.UserExistsByEmail(email))
        {
            throw new InvalidOperationException("User not found");
        }

        // Generate new verification token
        var emailToken = Guid.NewGuid().ToString();
        _emailVerificationTokens[emailToken] = email;
        
        // In a real implementation, this would send an email
        await Task.CompletedTask;
    }

    public string GenerateJwtToken(User user)
    {
        // For testing, return the same as GenerateToken
        return GenerateToken(user);
    }

    public string GenerateRefreshToken()
    {
        // Generate a simple refresh token
        return Guid.NewGuid().ToString();
    }
}