using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UPTRMS.Api.Services;
using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Tests.Mocks;

public class MockTokenService : ITokenService
{
    private readonly Dictionary<string, RefreshToken> _refreshTokens = new();

    public string GenerateAccessToken(User user)
    {
        // Generate a simple JWT-like token for testing
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            System.Text.Json.JsonSerializer.Serialize(new
            {
                sub = user.UserId.ToString(),
                email = user.Email,
                name = $"{user.FirstName} {user.LastName}",
                role = user.Role,
                exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            })));
        var signature = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("mock-signature"));
        
        return $"{header}.{payload}.{signature}";
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    public Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, string? deviceInfo = null)
    {
        var token = new RefreshToken
        {
            TokenId = Guid.NewGuid(),
            Token = GenerateRefreshToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            DeviceInfo = deviceInfo,
            IsRevoked = false
        };

        _refreshTokens[token.Token] = token;
        return Task.FromResult(token);
    }

    public Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
    {
        if (_refreshTokens.TryGetValue(token, out var refreshToken) && 
            !refreshToken.IsRevoked && 
            refreshToken.ExpiresAt > DateTime.UtcNow)
        {
            return Task.FromResult<RefreshToken?>(refreshToken);
        }
        
        return Task.FromResult<RefreshToken?>(null);
    }

    public Task RevokeRefreshTokenAsync(string token)
    {
        if (_refreshTokens.TryGetValue(token, out var refreshToken))
        {
            refreshToken.IsRevoked = true;
            // RefreshToken doesn't have RevokedAt property, just mark as revoked
        }
        
        return Task.CompletedTask;
    }

    public Task RevokeAllUserRefreshTokensAsync(Guid userId)
    {
        foreach (var token in _refreshTokens.Values)
        {
            if (token.UserId == userId && !token.IsRevoked)
            {
                token.IsRevoked = true;
                // RefreshToken doesn't have RevokedAt property, just mark as revoked
            }
        }
        
        return Task.CompletedTask;
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            // Simple token parsing for testing
            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            var payload = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
            var tokenData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(payload);

            if (tokenData == null)
                return null;

            var claims = new List<Claim>();
            
            if (tokenData.TryGetValue("sub", out var sub))
                claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.ToString()!));
                
            if (tokenData.TryGetValue("email", out var email))
                claims.Add(new Claim(ClaimTypes.Email, email.ToString()!));
                
            if (tokenData.TryGetValue("name", out var name))
                claims.Add(new Claim(ClaimTypes.Name, name.ToString()!));
                
            if (tokenData.TryGetValue("role", out var role))
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()!));

            var identity = new ClaimsIdentity(claims, "Bearer");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }

    public string GenerateEmailVerificationToken()
    {
        return $"verify-{Guid.NewGuid()}";
    }

    public string GeneratePasswordResetToken()
    {
        return $"reset-{Guid.NewGuid()}";
    }
}