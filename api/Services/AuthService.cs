using TherapyDocs.Api.Constants;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;

namespace TherapyDocs.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly IRegistrationAuditRepository _registrationAuditRepository;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly ILicenseVerificationService _licenseVerificationService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IRegistrationAuditRepository registrationAuditRepository,
        IPasswordService passwordService,
        IEmailService emailService,
        ILicenseVerificationService licenseVerificationService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _emailVerificationRepository = emailVerificationRepository;
        _registrationAuditRepository = registrationAuditRepository;
        _passwordService = passwordService;
        _emailService = emailService;
        _licenseVerificationService = licenseVerificationService;
        _logger = logger;
    }

    public async Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string? ipAddress, string? userAgent)
    {
        var response = new RegisterResponse();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var minimumResponseTime = SecurityConstants.MinimumRegistrationResponseTime;

        try
        {
            // Check if email already exists
            var emailExists = await _userRepository.EmailExistsAsync(request.Email);
            
            // Always perform all checks to prevent timing attacks
            var licenseExists = await _userRepository.LicenseExistsAsync(request.LicenseNumber, request.LicenseState);
            var isCommonPassword = _passwordService.IsCommonPassword(request.Password);
            
            // Hash password even if we won't use it (constant time)
            var hashedPassword = _passwordService.HashPassword(request.Password);
            
            if (emailExists)
            {
                response.Success = false;
                response.Message = "Registration failed";
                response.Errors.Add("Registration failed. Please check your information and try again.");
                
                await _registrationAuditRepository.LogRegistrationAttemptAsync(
                    request.Email, request.LicenseNumber, request.LicenseState, 
                    false, "Email already exists", ipAddress, userAgent);
                
                // Ensure constant response time
                var elapsed = stopwatch.Elapsed;
                if (elapsed < minimumResponseTime)
                {
                    await Task.Delay(minimumResponseTime - elapsed);
                }
                
                return response;
            }

            // Check if license already exists
            if (licenseExists)
            {
                response.Success = false;
                response.Message = "Registration failed";
                response.Errors.Add("Registration failed. Please check your information and try again.");
                
                await _registrationAuditRepository.LogRegistrationAttemptAsync(
                    request.Email, request.LicenseNumber, request.LicenseState, 
                    false, "License already exists", ipAddress, userAgent);
                
                // Ensure constant response time
                var elapsed = stopwatch.Elapsed;
                if (elapsed < minimumResponseTime)
                {
                    await Task.Delay(minimumResponseTime - elapsed);
                }
                
                return response;
            }

            // Check for common passwords
            if (isCommonPassword)
            {
                response.Success = false;
                response.Message = "Registration failed";
                response.Errors.Add("Password is too common. Please choose a more secure password.");
                
                await _registrationAuditRepository.LogRegistrationAttemptAsync(
                    request.Email, request.LicenseNumber, request.LicenseState, 
                    false, "Common password", ipAddress, userAgent);
                
                // Ensure constant response time
                var elapsed = stopwatch.Elapsed;
                if (elapsed < minimumResponseTime)
                {
                    await Task.Delay(minimumResponseTime - elapsed);
                }
                
                return response;
            }

            // Verify license with external API
            var licenseVerification = await _licenseVerificationService.VerifyLicenseAsync(
                request.LicenseNumber, request.LicenseState, request.LicenseType);

            if (!licenseVerification.Valid)
            {
                response.Success = false;
                response.Message = "Registration failed";
                response.Errors.Add(licenseVerification.ErrorMessage ?? "Registration failed. Please check your information and try again.");
                
                await _registrationAuditRepository.LogRegistrationAttemptAsync(
                    request.Email, request.LicenseNumber, request.LicenseState, 
                    false, "License verification failed", ipAddress, userAgent);
                
                // Ensure constant response time
                var elapsed = stopwatch.Elapsed;
                if (elapsed < minimumResponseTime)
                {
                    await Task.Delay(minimumResponseTime - elapsed);
                }
                
                return response;
            }

            // Check for disciplinary actions
            if (licenseVerification.DisciplinaryActions)
            {
                response.Success = false;
                response.Message = "Registration failed";
                response.Errors.Add("License has disciplinary actions. Please contact support for manual verification.");
                
                await _registrationAuditRepository.LogRegistrationAttemptAsync(
                    request.Email, request.LicenseNumber, request.LicenseState, 
                    false, "License has disciplinary actions", ipAddress, userAgent);
                
                // Ensure constant response time
                var elapsed = stopwatch.Elapsed;
                if (elapsed < minimumResponseTime)
                {
                    await Task.Delay(minimumResponseTime - elapsed);
                }
                
                return response;
            }

            // Map license type to service type
            var serviceType = MapLicenseTypeToServiceType(request.LicenseType);

            // Create user
            var user = new User
            {
                Email = request.Email,
                PasswordHash = hashedPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                LicenseNumber = request.LicenseNumber,
                LicenseState = request.LicenseState,
                ServiceType = serviceType,
                Status = "pending",
                EmailVerified = false
            };

            var userId = await _userRepository.CreateUserAsync(user);

            // Create email verification token
            var verificationToken = await _emailVerificationRepository.CreateVerificationTokenAsync(userId);

            // Send verification email
            var emailSent = await _emailService.SendVerificationEmailAsync(
                request.Email, request.FirstName, verificationToken);

            if (!emailSent)
            {
                _logger.LogWarning("Failed to send verification email for user: {UserId}", userId);
            }

            // Log successful registration
            await _registrationAuditRepository.LogRegistrationAttemptAsync(
                request.Email, request.LicenseNumber, request.LicenseState, 
                true, null, ipAddress, userAgent);

            response.Success = true;
            response.Message = "Registration successful! Please check your email to verify your account.";
            response.UserId = userId.ToString();

            // Ensure constant response time for successful registration too
            var successElapsed = stopwatch.Elapsed;
            if (successElapsed < minimumResponseTime)
            {
                await Task.Delay(minimumResponseTime - successElapsed);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            
            await _registrationAuditRepository.LogRegistrationAttemptAsync(
                request.Email, request.LicenseNumber, request.LicenseState, 
                false, "System error", ipAddress, userAgent);

            response.Success = false;
            response.Message = "An error occurred during registration. Please try again later.";
            response.Errors.Add("System error occurred");
            
            // Ensure constant response time even for errors
            var errorElapsed = stopwatch.Elapsed;
            if (errorElapsed < minimumResponseTime)
            {
                await Task.Delay(minimumResponseTime - errorElapsed);
            }
            
            return response;
        }
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        try
        {
            var verificationToken = await _emailVerificationRepository.GetTokenAsync(token);
            
            if (verificationToken == null)
            {
                _logger.LogWarning("Invalid verification token attempted");
                return false;
            }

            if (verificationToken.UsedAt.HasValue)
            {
                _logger.LogWarning("Verification token already used");
                return false;
            }

            if (verificationToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Verification token expired");
                return false;
            }

            // Mark token as used
            var tokenMarked = await _emailVerificationRepository.MarkTokenUsedAsync(token);
            if (!tokenMarked)
            {
                return false;
            }

            // Verify user email
            var verified = await _userRepository.VerifyEmailAsync(verificationToken.UserId);
            if (!verified)
            {
                return false;
            }

            // Get user details for welcome email
            var user = await _userRepository.GetUserByIdAsync(verificationToken.UserId);
            if (user != null)
            {
                // Update user status
                user.EmailVerified = true;
                user.Status = "active";
                await _userRepository.UpdateUserAsync(user);

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email with token");
            return false;
        }
    }

    public async Task<bool> ResendVerificationEmailAsync(string email)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var minimumResponseTime = SecurityConstants.MinimumEmailVerificationResponseTime;
        
        try
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            
            // Always perform all operations to maintain constant time
            bool result = false;
            
            if (user != null && !user.EmailVerified)
            {
                // Check if user already has a valid token
                var hasValidToken = await _emailVerificationRepository.HasValidTokenAsync(user.Id);
                
                if (!hasValidToken)
                {
                    // Create new verification token
                    var verificationToken = await _emailVerificationRepository.CreateVerificationTokenAsync(user.Id);

                    // Send verification email
                    result = await _emailService.SendVerificationEmailAsync(user.Email, user.FirstName, verificationToken);
                }
                else
                {
                    _logger.LogInformation("User already has valid verification token");
                }
            }
            
            // Ensure constant response time
            var elapsed = stopwatch.Elapsed;
            if (elapsed < minimumResponseTime)
            {
                await Task.Delay(minimumResponseTime - elapsed);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification email");
            
            // Ensure constant response time even on error
            var errorElapsed = stopwatch.Elapsed;
            if (errorElapsed < minimumResponseTime)
            {
                await Task.Delay(minimumResponseTime - errorElapsed);
            }
            
            return false;
        }
    }

    private static string MapLicenseTypeToServiceType(string licenseType)
    {
        return licenseType.ToUpper() switch
        {
            "OT" => "occupational_therapy",
            "PT" => "physical_therapy",
            "SLP" => "speech_therapy",
            _ => "speech_therapy" // Default
        };
    }
}