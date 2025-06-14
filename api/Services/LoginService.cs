using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;

namespace TherapyDocs.Api.Services;

public class LoginService : ILoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IAccountLockoutRepository _lockoutRepository;
    private readonly IPasswordHistoryRepository _passwordHistoryRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginService> _logger;

    public LoginService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IAccountLockoutRepository lockoutRepository,
        IPasswordHistoryRepository passwordHistoryRepository,
        IConfiguration configuration,
        ILogger<LoginService> logger)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _lockoutRepository = lockoutRepository;
        _passwordHistoryRepository = passwordHistoryRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var minimumResponseTime = TimeSpan.FromMilliseconds(500); // Constant time to prevent timing attacks

        try
        {
            // Check account lockout status
            var lockoutStatus = await _lockoutRepository.CheckAccountLockoutAsync(request.Email);
            
            // Get user (even if locked, to maintain constant time)
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            
            // Always verify password to maintain constant time
            bool passwordValid = false;
            if (user != null)
            {
                passwordValid = _passwordService.VerifyPassword(request.Password, user.PasswordHash);
            }
            else
            {
                // Perform a dummy password verification to maintain constant time
                _passwordService.VerifyPassword(request.Password, "$2a$12$dummy.hash.to.maintain.constant.time");
            }

            // Check if account is locked
            if (lockoutStatus.IsLocked)
            {
                await DelayResponse(stopwatch, minimumResponseTime);
                
                var minutesRemaining = lockoutStatus.LockedUntil.HasValue 
                    ? (int)(lockoutStatus.LockedUntil.Value - DateTime.UtcNow).TotalMinutes
                    : 0;
                
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Account is locked. Please try again in {minutesRemaining} minutes.",
                    IsLocked = true,
                    RemainingAttempts = 0
                };
            }

            // Check if login is valid
            if (user == null || !passwordValid)
            {
                // Record failed attempt
                await _lockoutRepository.RecordFailedLoginAttemptAsync(request.Email, ipAddress, userAgent);
                
                // Get updated lockout status
                lockoutStatus = await _lockoutRepository.CheckAccountLockoutAsync(request.Email);
                
                await DelayResponse(stopwatch, minimumResponseTime);
                
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid email or password.",
                    RemainingAttempts = lockoutStatus.RemainingAttempts,
                    IsLocked = lockoutStatus.IsLocked
                };
            }

            // Check if email is verified
            if (!user.EmailVerified)
            {
                await DelayResponse(stopwatch, minimumResponseTime);
                
                return new LoginResponse
                {
                    Success = false,
                    Message = "Please verify your email address before logging in.",
                    RequiresEmailVerification = true
                };
            }

            // Check if account is active
            if (user.Status != "active")
            {
                await DelayResponse(stopwatch, minimumResponseTime);
                
                return new LoginResponse
                {
                    Success = false,
                    Message = "Your account is not active. Please contact support."
                };
            }

            // Clear failed login attempts on successful login
            await _lockoutRepository.ClearFailedLoginAttemptsAsync(request.Email);

            // Check if password change is required
            var passwordRequirement = await _passwordHistoryRepository.CheckPasswordChangeRequiredAsync(user.Id);

            // Generate JWT token
            var token = GenerateJwtToken(user);

            await DelayResponse(stopwatch, minimumResponseTime);

            var response = new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ServiceType = user.ServiceType
                }
            };

            // Add password expiry warning if needed
            if (passwordRequirement.ChangeRequired)
            {
                response.PasswordChangeRequired = true;
                response.Message = "Login successful. Your password has expired and must be changed.";
            }
            else if (passwordRequirement.DaysUntilExpiry <= 14) // Warning period
            {
                response.PasswordExpiryWarning = true;
                response.DaysUntilPasswordExpiry = passwordRequirement.DaysUntilExpiry;
                response.Message = $"Login successful. Your password will expire in {passwordRequirement.DaysUntilExpiry} days.";
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            
            await DelayResponse(stopwatch, minimumResponseTime);
            
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login. Please try again later."
            };
        }
    }

    private async Task DelayResponse(System.Diagnostics.Stopwatch stopwatch, TimeSpan minimumResponseTime)
    {
        var elapsed = stopwatch.Elapsed;
        if (elapsed < minimumResponseTime)
        {
            await Task.Delay(minimumResponseTime - elapsed);
        }
    }

    private string GenerateJwtToken(Models.User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim("service_type", user.ServiceType),
            new Claim("license_number", user.LicenseNumber),
            new Claim("license_state", user.LicenseState)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8), // 8 hour token lifetime
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}