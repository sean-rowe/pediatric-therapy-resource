using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Repositories;

namespace TherapyDocs.Api.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailVerificationService> _logger;

    public EmailVerificationService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IEmailService emailService,
        ILogger<EmailVerificationService> logger)
    {
        _userRepository = userRepository;
        _emailVerificationRepository = emailVerificationRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(int userId, string email, string firstName)
    {
        // Create email verification token
        var verificationToken = await _emailVerificationRepository.CreateVerificationTokenAsync(userId);

        // Send verification email
        var emailSent = await _emailService.SendVerificationEmailAsync(email, firstName, verificationToken);

        if (!emailSent)
        {
            _logger.LogWarning("Failed to send verification email for user: {UserId}", userId);
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
        var minimumResponseTime = TimeSpan.FromMilliseconds(300); // Constant time to prevent timing attacks
        
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
}