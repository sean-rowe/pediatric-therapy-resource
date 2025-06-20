using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using TherapyDocs.Api.Interfaces;

namespace TherapyDocs.Api.Services;

public class EmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var apiKey = _configuration["SendGrid:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("SendGrid API key not configured");
        }

        _sendGridClient = new SendGridClient(apiKey);
    }

    public async Task<bool> SendVerificationEmailAsync(string email, string firstName, string verificationToken)
    {
        try
        {
            var fromEmail = new EmailAddress(_configuration["SendGrid:FromEmail"], "TherapyDocs");
            var toEmail = new EmailAddress(email, firstName);
            var baseUrl = _configuration["Application:BaseUrl"] ?? "https://localhost:3000";
            var verificationUrl = $"{baseUrl}/verify-email?token={verificationToken}";

            var subject = "Verify Your TherapyDocs Account";
            var plainTextContent = $@"
Dear {firstName},

Welcome to TherapyDocs! Please verify your email address to activate your account.

Click the following link to verify your email:
{verificationUrl}

This link will expire in 24 hours. If you didn't create this account, please ignore this email.

Best regards,
The TherapyDocs Team";

            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to TherapyDocs!</h1>
        </div>
        <div class='content'>
            <p>Dear {firstName},</p>
            <p>Thank you for registering with TherapyDocs. Please verify your email address to activate your account.</p>
            <center>
                <a href='{verificationUrl}' class='button'>Verify Email</a>
            </center>
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all;'>{verificationUrl}</p>
            <p>This link will expire in 24 hours. If you didn't create this account, please ignore this email.</p>
        </div>
        <div class='footer'>
            <p>Best regards,<br>The TherapyDocs Team</p>
            <p>© 2024 TherapyDocs. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            var msg = MailHelper.CreateSingleEmail(fromEmail, toEmail, subject, plainTextContent, htmlContent);
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Verification email sent successfully to: {Email}", email);
                return true;
            }

            _logger.LogWarning("Failed to send verification email. Status: {Status}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification email to: {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
    {
        try
        {
            var fromEmail = new EmailAddress(_configuration["SendGrid:FromEmail"], "TherapyDocs");
            var toEmail = new EmailAddress(email, firstName);
            var loginUrl = $"{_configuration["Application:BaseUrl"]}/login";

            var subject = "Welcome to TherapyDocs!";
            var plainTextContent = $@"
Dear {firstName},

Your account has been successfully verified! You can now log in and start using TherapyDocs.

Log in at: {loginUrl}

Next steps:
- Complete your profile
- Set up your therapy types
- Configure your schedule

Need help? Visit our help center or contact support.

Best regards,
The TherapyDocs Team";

            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .steps {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #4CAF50; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to TherapyDocs!</h1>
        </div>
        <div class='content'>
            <p>Dear {firstName},</p>
            <p>Congratulations! Your account has been successfully verified. You can now log in and start using TherapyDocs to streamline your therapy documentation.</p>
            <center>
                <a href='{loginUrl}' class='button'>Log In Now</a>
            </center>
            <div class='steps'>
                <h3>Next steps:</h3>
                <ul>
                    <li>Complete your profile</li>
                    <li>Set up your therapy types</li>
                    <li>Configure your schedule</li>
                    <li>Start documenting sessions</li>
                </ul>
            </div>
            <p>Need help? Visit our <a href='{_configuration["Application:BaseUrl"]}/help'>help center</a> or contact our support team.</p>
        </div>
        <div class='footer'>
            <p>Best regards,<br>The TherapyDocs Team</p>
            <p>© 2024 TherapyDocs. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            var msg = MailHelper.CreateSingleEmail(fromEmail, toEmail, subject, plainTextContent, htmlContent);
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Welcome email sent successfully to: {Email}", email);
                return true;
            }

            _logger.LogWarning("Failed to send welcome email. Status: {Status}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to: {Email}", email);
            return false;
        }
    }
}