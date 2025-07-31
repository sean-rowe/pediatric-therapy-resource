using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Services;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Basic validation for required fields
        if (!ModelState.IsValid)
        {
            // Extract specific validation errors
            string? firstError = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => x.Value?.Errors.First().ErrorMessage)
                .FirstOrDefault();

            return BadRequest(new { error = firstError ?? "Invalid request data" });
        }

        try
        {
            (User user, string token, string refreshToken) = await _authService.RegisterAsync(request);

            // Return response in expected format
            return Ok(new
            {
                success = true,
                message = "Registration successful. Please check your email to verify your account.",
                userId = user.UserId.ToString(),
                token = token,
                refreshToken = refreshToken
            });
        }
        catch (InvalidOperationException ex)
        {
            // Handle specific registration errors
            if (ex.Message.Contains("already exists"))
            {
                return BadRequest(new { error = "Email already registered" });
            }
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { error = "An error occurred during registration" });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Log the incoming request for debugging
        _logger.LogInformation("Login request received for email: {Email}", request?.Email ?? "null");

        // Log the request object state
        if (request == null)
        {
            _logger.LogWarning("Login request is null");
            return BadRequest(new { error = "Invalid request data" });
        }

        _logger.LogInformation("Login request - Email: {Email}, Password length: {Length}, Has TwoFactorCode: {HasCode}",
            request.Email ?? "null",
            request.Password?.Length ?? 0,
            !string.IsNullOrEmpty(request.TwoFactorCode));

        // Check model state before processing
        if (!ModelState.IsValid)
        {
            Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry> kvp in ModelState)
            {
                if (kvp.Value.Errors.Count > 0)
                {
                    foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in kvp.Value.Errors)
                    {
                        _logger.LogWarning("Validation error for field '{Field}': {Error}", kvp.Key, error.ErrorMessage);
                    }
                }
            }
            return BadRequest(new { error = "Invalid request data" });
        }

        try
        {
            (User user, string token, string refreshToken) = await _authService.LoginAsync(request);

            // Return successful response with expected fields
            return Ok(new
            {
                success = true,
                token = token,
                refreshToken = refreshToken,
                user = new
                {
                    userId = user.UserId,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role.ToString(),
                    subscriptionTier = user.SubscriptionTier.ToString()
                }
            });
        }
        catch (UnauthorizedAccessException)
        {
            // Return consistent error message for invalid credentials
            return BadRequest(new { error = "Invalid credentials" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "An error occurred during login" });
        }
    }

    [HttpGet("verify-email/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        try
        {
            bool success = await _authService.VerifyEmailAsync(token);

            if (success)
            {
                return Ok(new { success = true, message = "Email verified successfully" });
            }
            else
            {
                return BadRequest(new { success = false, message = "Invalid or expired verification token" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return StatusCode(500, new { error = "An error occurred during email verification" });
        }
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        try
        {
            await _authService.ResendVerificationEmailAsync(request.Email);
            return Ok(new { message = "Verification email sent if account exists" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification email");
            return StatusCode(500, new { error = "An error occurred while resending verification email" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Get the token from the Authorization header
            string? authHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return BadRequest(new { error = "Invalid authorization header" });
            }

            string token = authHeader.Substring("Bearer ".Length).Trim();
            await _authService.LogoutAsync(token);

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { error = "An error occurred during logout" });
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            await _authService.GeneratePasswordResetTokenAsync(request.Email);
            // Always return success to prevent user enumeration
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            await _authService.GeneratePasswordResetTokenAsync(request.Email);
            // Always return success to prevent user enumeration
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset request");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    [HttpPost("reset-password/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordConfirmRequest request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
            return Ok(new { message = "Password reset successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset confirmation");
            return StatusCode(500, new { error = "An error occurred while resetting password" });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            // Get user ID from claims
            string? userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new { error = "Invalid user authentication" });
            }

            await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new { error = "An error occurred while changing password" });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            (string token, string refreshToken) = await _authService.RefreshTokenAsync(request.RefreshToken);

            return Ok(new
            {
                token = token,
                refreshToken = refreshToken
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new { error = "An error occurred while refreshing token" });
        }
    }

    [HttpGet("sso/providers")]
    [AllowAnonymous]
    public IActionResult GetSsoProviders()
    {
        object[] providers = new[]
        {
            new { id = "google", name = "Google", type = "oauth2", enabled = true },
            new { id = "clever", name = "Clever", type = "saml", enabled = true },
            new { id = "classlink", name = "ClassLink", type = "oauth2", enabled = true }
        };

        return Ok(providers);
    }
}