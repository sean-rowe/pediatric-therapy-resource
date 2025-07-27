using System;
using System.Threading.Tasks;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Services;

namespace UPTRMS.Api.Tests.Mocks;

public class MockAuthenticationService : IAuthenticationService
{
    // Make these static so they persist across scoped instances
    private static readonly Dictionary<string, User> _users = new();
    private static readonly Dictionary<string, string> _resetTokens = new();
    private static readonly HashSet<string> _blacklistedTokens = new();
    
    // Static method to clear state between tests if needed
    public static void ClearState()
    {
        _users.Clear();
        _resetTokens.Clear();
        _blacklistedTokens.Clear();
    }

    public Task<(User user, string token, string refreshToken)> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        if (_users.Values.Any(u => u.Email == request.Email))
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            LicenseNumber = request.LicenseNumber,
            SubscriptionTier = SubscriptionTier.Free,
            Role = UserRole.Therapist,
            Languages = new List<string> { "English" },
            Specialties = request.Specialties.ToList(),
            IsSellerApproved = false,
            EmailVerified = false,
            TwoFactorEnabled = false,
            PreferredLanguage = "en",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _users[user.Email] = user;

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        return Task.FromResult((user, token, refreshToken));
    }

    public Task<(User user, string token, string refreshToken)> LoginAsync(LoginRequest request)
    {
        Console.WriteLine($"[MockAuthenticationService] LoginAsync called with email: {request?.Email ?? "null"}");
        
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        
        if (!_users.TryGetValue(request.Email, out User? user))
        {
            Console.WriteLine($"[MockAuthenticationService] User not found: {request.Email}");
            Console.WriteLine($"[MockAuthenticationService] Available users: {string.Join(", ", _users.Keys)}");
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        Console.WriteLine($"[MockAuthenticationService] User found, verifying password");
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            Console.WriteLine($"[MockAuthenticationService] Password verification failed");
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        Console.WriteLine($"[MockAuthenticationService] Password verified, checking email verification status: {user.EmailVerified}");
        if (!user.EmailVerified)
        {
            throw new UnauthorizedAccessException("Email not verified");
        }

        if (user.TwoFactorEnabled && string.IsNullOrEmpty(request.TwoFactorCode))
        {
            throw new InvalidOperationException("Two-factor authentication required");
        }

        string token = GenerateJwtToken(user);
        string refreshToken = GenerateRefreshToken();

        return Task.FromResult((user, token, refreshToken));
    }

    public Task<bool> VerifyEmailAsync(string token)
    {
        // In a real implementation, this would verify the token
        // For testing, we'll accept any non-empty token
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(false);
        }

        // Find and verify the user (simplified for testing)
        // For testing purposes, verify the most recently created unverified user
        User? user = _users.Values
            .Where(u => !u.EmailVerified)
            .OrderByDescending(u => u.CreatedAt)
            .FirstOrDefault();
            
        if (user != null)
        {
            user.EmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            Console.WriteLine($"[MockAuthenticationService] Email verified for user: {user.Email}");
        }

        return Task.FromResult(user != null);
    }
    
    // Helper method for testing - verify by email
    public Task<bool> VerifyEmailByEmailAsync(string email)
    {
        if (_users.TryGetValue(email, out User? user))
        {
            user.EmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            Console.WriteLine($"[MockAuthenticationService] Email verified for user: {user.Email}");
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task ResendVerificationEmailAsync(string email)
    {
        // In testing, we just check if the user exists
        if (!_users.ContainsKey(email))
        {
            // Don't throw to prevent email enumeration
            return Task.CompletedTask;
        }

        // In real implementation, would send email
        return Task.CompletedTask;
    }

    public Task LogoutAsync(string token)
    {
        // Add token to blacklist
        _blacklistedTokens.Add(token);
        return Task.CompletedTask;
    }

    public Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var resetToken = Guid.NewGuid().ToString();
        if (_users.ContainsKey(email))
        {
            _resetTokens[resetToken] = email;
        }
        // Return token but don't throw to prevent email enumeration
        return Task.FromResult(resetToken);
    }

    public Task ResetPasswordAsync(string token, string newPassword)
    {
        if (!_resetTokens.TryGetValue(token, out var email))
        {
            throw new InvalidOperationException("Invalid or expired reset token");
        }

        if (_users.TryGetValue(email, out var user))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            _resetTokens.Remove(token);
        }

        return Task.CompletedTask;
    }

    public Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = _users.Values.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        return Task.CompletedTask;
    }

    public Task<(string token, string refreshToken)> RefreshTokenAsync(string refreshToken)
    {
        // Simplified for testing
        var user = _users.Values.FirstOrDefault();
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var newToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        return Task.FromResult((newToken, newRefreshToken));
    }

    public Task<User?> GetUserByIdAsync(Guid userId)
    {
        var user = _users.Values.FirstOrDefault(u => u.UserId == userId);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        _users.TryGetValue(email, out var user);
        return Task.FromResult(user);
    }

    public Task<bool> IsTokenBlacklistedAsync(string token)
    {
        return Task.FromResult(_blacklistedTokens.Contains(token));
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        // Check if token is blacklisted
        if (_blacklistedTokens.Contains(token))
        {
            return Task.FromResult(false);
        }
        
        // Simplified validation for testing
        return Task.FromResult(token.StartsWith("test-jwt-token-"));
    }

    public string GenerateJwtToken(User user)
    {
        // Simplified token generation for testing
        return $"test-jwt-token-{user.UserId}";
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }
}