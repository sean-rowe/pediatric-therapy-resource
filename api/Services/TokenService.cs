using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Services;

public class TokenService : ITokenService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private readonly string _jwtSecret;
    private readonly string _issuer;
    private readonly string _audience;

    public TokenService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<TokenService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _jwtSecret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        _issuer = _configuration["Jwt:Issuer"] ?? "UPTRMS";
        _audience = _configuration["Jwt:Audience"] ?? "UPTRMS";
    }

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("subscription_tier", user.SubscriptionTier.ToString())
        };

        if (user.IsSellerApproved)
        {
            claims.Add(new Claim("is_seller", "true"));
        }

        if (user.OrganizationId.HasValue)
        {
            claims.Add(new Claim("organization_id", user.OrganizationId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, string? deviceInfo = null)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            DeviceInfo = deviceInfo
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return null;
        }

        return refreshToken;
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllUserRefreshTokensAsync(Guid userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync();
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    public string GenerateEmailVerificationToken()
    {
        return GenerateSecureToken();
    }

    public string GeneratePasswordResetToken()
    {
        return GenerateSecureToken();
    }

    private string GenerateSecureToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}