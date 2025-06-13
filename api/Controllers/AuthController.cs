using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Services;
using FluentValidation;

namespace TherapyDocs.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService, 
        IValidator<RegisterRequest> registerValidator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new therapist user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Registration result</returns>
    [HttpPost("register")]
    [EnableRateLimiting("registration")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Validate request
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var response = new RegisterResponse
            {
                Success = false,
                Message = "Validation failed",
                Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            };
            return BadRequest(response);
        }

        // Get client IP and user agent for audit
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        // Register user
        var result = await _authService.RegisterUserAsync(request, ipAddress, userAgent);

        if (result.Success)
        {
            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Verify email address using token
    /// </summary>
    /// <param name="token">Verification token from email</param>
    /// <returns>Verification result</returns>
    [HttpGet("verify-email/{token}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { success = false, message = "Invalid verification token" });
        }

        var result = await _authService.VerifyEmailAsync(token);

        if (result)
        {
            _logger.LogInformation("Email verified successfully with token: {Token}", token);
            return Ok(new { success = true, message = "Email verified successfully! You can now log in." });
        }

        return BadRequest(new { success = false, message = "Invalid or expired verification token" });
    }

    /// <summary>
    /// Resend email verification
    /// </summary>
    /// <param name="request">Email to resend verification to</param>
    /// <returns>Result of resend operation</returns>
    [HttpPost("resend-verification")]
    [EnableRateLimiting("registration")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { success = false, message = "Email is required" });
        }

        var result = await _authService.ResendVerificationEmailAsync(request.Email);

        if (result)
        {
            _logger.LogInformation("Verification email resent to: {Email}", request.Email);
            return Ok(new { success = true, message = "Verification email sent successfully" });
        }

        return BadRequest(new { success = false, message = "Unable to resend verification email. Please check your email address." });
    }
}

public class ResendVerificationRequest
{
    public string Email { get; set; } = string.Empty;
}