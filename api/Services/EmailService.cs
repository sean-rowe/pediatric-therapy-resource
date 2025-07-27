namespace UPTRMS.Api.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendVerificationEmailAsync(string email, string firstName, string token)
    {
        var verificationUrl = $"{_configuration["App:BaseUrl"]}/verify-email?token={token}";
        
        _logger.LogInformation("Sending verification email to {Email}", email);
        
        // In production, integrate with SendGrid or similar service
        // For now, just log the action
        await Task.CompletedTask;
    }

    public async Task SendPasswordResetEmailAsync(string email, string firstName, string token)
    {
        var resetUrl = $"{_configuration["App:BaseUrl"]}/reset-password?token={token}";
        
        _logger.LogInformation("Sending password reset email to {Email}", email);
        
        await Task.CompletedTask;
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName)
    {
        _logger.LogInformation("Sending welcome email to {Email}", email);
        await Task.CompletedTask;
    }

    public async Task SendInvitationEmailAsync(string email, string inviterName, string organizationName, string inviteCode)
    {
        _logger.LogInformation("Sending invitation email to {Email} from {Organization}", email, organizationName);
        await Task.CompletedTask;
    }

    public async Task SendResourceSharedEmailAsync(string email, string recipientName, string senderName, string resourceTitle, string accessLink)
    {
        _logger.LogInformation("Sending resource share notification to {Email}", email);
        await Task.CompletedTask;
    }

    public async Task SendPaymentReceiptAsync(string email, string firstName, decimal amount, string description)
    {
        _logger.LogInformation("Sending payment receipt to {Email} for ${Amount}", email, amount);
        await Task.CompletedTask;
    }

    public async Task SendSellerNotificationAsync(string email, string sellerName, string notificationType, string details)
    {
        _logger.LogInformation("Sending seller notification to {Email}: {Type}", email, notificationType);
        await Task.CompletedTask;
    }
}