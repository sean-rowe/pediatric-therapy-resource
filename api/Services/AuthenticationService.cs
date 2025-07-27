using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;

namespace UPTRMS.Api.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IPasswordHasher<User> passwordHasher,
        IEmailService emailService,
        ILogger<AuthenticationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<(User user, string token, string refreshToken)> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create new user
        User user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            LicenseNumber = request.LicenseNumber,
            Languages = request.Languages,
            Specialties = request.Specialties,
            Role = request.Role,
            SubscriptionTier = SubscriptionTier.Free
        };

        // Hash password
        string hashedPassword = _passwordHasher.HashPassword(user, request.Password);
        user.PasswordHash = hashedPassword;
        user.EmailVerified = false; // Will be set to true after email verification

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Generate tokens
        string token = GenerateJwtToken(user);
        string refreshToken = GenerateRefreshToken();
        
        // Store refresh token
        await StoreRefreshTokenAsync(user.UserId, refreshToken);

        // Send verification email
        await SendVerificationEmailAsync(user);

        return (user, token, refreshToken);
    }

    public async Task<(User user, string token, string refreshToken)> LoginAsync(LoginRequest request)
    {
        User? user = await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Verify password
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Check if 2FA is enabled and code is required
        if (user.TwoFactorEnabled && string.IsNullOrEmpty(request.TwoFactorCode))
        {
            throw new InvalidOperationException("Two-factor authentication code required");
        }

        // TODO: Verify 2FA code if provided

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generate tokens
        string token = GenerateJwtToken(user);
        string refreshToken = GenerateRefreshToken();
        
        // Store refresh token
        await StoreRefreshTokenAsync(user.UserId, refreshToken);

        return (user, token, refreshToken);
    }

    public string GenerateJwtToken(User user)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured"));
        
        List<Claim> claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("subscription_tier", user.SubscriptionTier.ToString())
        };

        if (user.OrganizationId.HasValue)
        {
            claims.Add(new Claim("organization_id", user.OrganizationId.Value.ToString()));
        }

        if (user.IsSellerApproved)
        {
            claims.Add(new Claim("is_seller", "true"));
        }

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[32];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]!);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<(string token, string refreshToken)> RefreshTokenAsync(string refreshToken)
    {
        // Find the refresh token in the database
        RefreshToken? storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (storedToken == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Check if token is expired
        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token has expired");
        }

        // Revoke the old refresh token
        storedToken.IsRevoked = true;
        
        // Generate new tokens
        string newToken = GenerateJwtToken(storedToken.User);
        string newRefreshToken = GenerateRefreshToken();
        
        // Store the new refresh token
        await StoreRefreshTokenAsync(storedToken.UserId, newRefreshToken);
        
        // Save changes to revoke old token
        await _context.SaveChangesAsync();

        return (newToken, newRefreshToken);
    }

    public async Task LogoutAsync(string token)
    {
        // TODO: Implement token blacklisting
        await Task.CompletedTask;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            // Don't reveal if user exists
            return string.Empty;
        }

        string token = GenerateSecureToken();
        await StorePasswordResetTokenAsync(user.UserId, token);
        
        // Send email
        await _emailService.SendPasswordResetEmailAsync(user.Email, user.FirstName, token);
        
        return token;
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        // Find the password reset token
        PasswordResetToken? resetToken = await _context.PasswordResetTokens
            .Include(prt => prt.User)
            .FirstOrDefaultAsync(prt => prt.Token == token && !prt.IsUsed);

        if (resetToken == null)
        {
            throw new InvalidOperationException("Invalid or expired reset token");
        }

        // Check if token is expired
        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Reset token has expired");
        }

        // Update the user's password
        string newHashedPassword = _passwordHasher.HashPassword(resetToken.User, newPassword);
        resetToken.User.PasswordHash = newHashedPassword;
        
        // Mark token as used
        resetToken.IsUsed = true;
        
        // Save changes
        await _context.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        User? user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
        
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        string newHashedPassword = _passwordHasher.HashPassword(user, newPassword);
        user.PasswordHash = newHashedPassword;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        // Find the email verification token
        EmailVerificationToken? verificationToken = await _context.EmailVerificationTokens
            .Include(evt => evt.User)
            .FirstOrDefaultAsync(evt => evt.Token == token && !evt.IsUsed);

        if (verificationToken == null)
        {
            return false;
        }

        // Check if token is expired
        if (verificationToken.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        // Mark email as verified
        verificationToken.User.EmailVerified = true;
        
        // Mark token as used
        verificationToken.IsUsed = true;
        
        // Save changes
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task ResendVerificationEmailAsync(string email)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || user.EmailVerified)
        {
            return; // Don't reveal if user exists or is already verified
        }

        await SendVerificationEmailAsync(user);
    }

    private async Task SendVerificationEmailAsync(User user)
    {
        string token = GenerateSecureToken();
        await StoreEmailVerificationTokenAsync(user.UserId, token);
        await _emailService.SendVerificationEmailAsync(user.Email, user.FirstName, token);
    }

    private string GenerateSecureToken()
    {
        byte[] randomNumber = new byte[32];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber).Replace("/", "-").Replace("+", "_");
    }

    private async Task StoreRefreshTokenAsync(Guid userId, string token)
    {
        RefreshToken refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    private async Task StorePasswordResetTokenAsync(Guid userId, string token)
    {
        PasswordResetToken resetToken = new PasswordResetToken
        {
            TokenId = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            IsUsed = false
        };

        await _context.PasswordResetTokens.AddAsync(resetToken);
        await _context.SaveChangesAsync();
    }

    private async Task StoreEmailVerificationTokenAsync(Guid userId, string token)
    {
        EmailVerificationToken verificationToken = new EmailVerificationToken
        {
            TokenId = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsUsed = false
        };

        await _context.EmailVerificationTokens.AddAsync(verificationToken);
        await _context.SaveChangesAsync();
    }
}