using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Models.DTOs;

namespace TherapyDocs.Api.Services;

// Refactored AuthService that delegates to specialized services
public class AuthServiceRefactored : IAuthService
{
    private readonly IUserRegistrationService _registrationService;
    private readonly IEmailVerificationService _verificationService;

    public AuthServiceRefactored(
        IUserRegistrationService registrationService,
        IEmailVerificationService verificationService)
    {
        _registrationService = registrationService;
        _verificationService = verificationService;
    }

    public async Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string? ipAddress, string? userAgent)
    {
        return await _registrationService.RegisterUserAsync(request, ipAddress, userAgent);
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        return await _verificationService.VerifyEmailAsync(token);
    }

    public async Task<bool> ResendVerificationEmailAsync(string email)
    {
        return await _verificationService.ResendVerificationEmailAsync(email);
    }
}