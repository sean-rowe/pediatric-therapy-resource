using TherapyDocs.Api.Constants;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;

namespace TherapyDocs.Api.Services;

public interface IUserRegistrationService
{
    Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string? ipAddress, string? userAgent);
}

public class UserRegistrationService : IUserRegistrationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRegistrationAuditRepository _registrationAuditRepository;
    private readonly IPasswordService _passwordService;
    private readonly ILicenseVerificationService _licenseVerificationService;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IPasswordHistoryRepository _passwordHistoryRepository;
    private readonly ILogger<UserRegistrationService> _logger;

    public UserRegistrationService(
        IUserRepository userRepository,
        IRegistrationAuditRepository registrationAuditRepository,
        IPasswordService passwordService,
        ILicenseVerificationService licenseVerificationService,
        IEmailVerificationService emailVerificationService,
        IPasswordHistoryRepository passwordHistoryRepository,
        ILogger<UserRegistrationService> logger)
    {
        _userRepository = userRepository;
        _registrationAuditRepository = registrationAuditRepository;
        _passwordService = passwordService;
        _licenseVerificationService = licenseVerificationService;
        _emailVerificationService = emailVerificationService;
        _passwordHistoryRepository = passwordHistoryRepository;
        _logger = logger;
    }

    public async Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string? ipAddress, string? userAgent)
    {
        // Input validation
        ArgumentNullException.ThrowIfNull(request);
        
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

            // Add initial password to history
            await _passwordHistoryRepository.AddPasswordToHistoryAsync(userId, hashedPassword);

            // Create and send verification email
            await _emailVerificationService.SendVerificationEmailAsync(userId, request.Email, request.FirstName);

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