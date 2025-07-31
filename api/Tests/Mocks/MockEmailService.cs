using System.Collections.Generic;
using System.Threading.Tasks;
using UPTRMS.Api.Services;

namespace UPTRMS.Api.Tests.Mocks;

public class MockEmailService : IEmailService
{
    private readonly List<SentEmail> _sentEmails = new();

    public Task SendVerificationEmailAsync(string email, string firstName, string token)
    {
        _sentEmails.Add(new SentEmail
        {
            To = email,
            Subject = "Verify Your Email",
            Body = $"Hi {firstName}, please verify your email using token: {token}",
            IsHtml = true,
            TemplateName = "EmailVerification"
        });
        
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string email, string firstName, string token)
    {
        _sentEmails.Add(new SentEmail
        {
            To = email,
            Subject = "Reset Your Password",
            Body = $"Hi {firstName}, reset your password using token: {token}",
            IsHtml = true,
            TemplateName = "PasswordReset"
        });
        
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string email, string firstName)
    {
        _sentEmails.Add(new SentEmail
        {
            To = email,
            Subject = "Welcome to UPTRMS",
            Body = $"Hi {firstName}, welcome to UPTRMS!",
            IsHtml = true,
            TemplateName = "Welcome"
        });
        
        return Task.CompletedTask;
    }

    public Task SendInvitationEmailAsync(string email, string inviterName, string organizationName, string inviteCode)
    {
        _sentEmails.Add(new SentEmail
        {
            To = email,
            Subject = $"Invitation to join {organizationName}",
            Body = $"{inviterName} has invited you to join {organizationName}. Use code: {inviteCode}",
            IsHtml = true,
            TemplateName = "Invitation"
        });
        
        return Task.CompletedTask;
    }

    public Task SendResourceSharedEmailAsync(string email, string recipientName, string senderName, string resourceTitle, string accessLink)
    {
        _sentEmails.Add(new SentEmail
        {
            To = email,
            Subject = $"{senderName} shared a resource with you",
            Body = $"Hi {recipientName}, {senderName} has shared '{resourceTitle}' with you. Access it here: {accessLink}",
            IsHtml = true,
            TemplateName = "ResourceShared"
        });
        
        return Task.CompletedTask;
    }

    public Task SendPaymentReceiptAsync(string email, string firstName, decimal amount, string description)
    {
        _sentEmails.Add(new SentEmail
        {
            To = email,
            Subject = "Payment Receipt",
            Body = $"Hi {firstName}, your payment of ${amount} for {description} has been processed.",
            IsHtml = true,
            TemplateName = "PaymentReceipt"
        });
        
        return Task.CompletedTask;
    }

    public Task SendSellerNotificationAsync(string email, string sellerName, string notificationType, string details)
    {
        _sentEmails.Add(new SentEmail
        {
            To = email,
            Subject = $"Seller Notification: {notificationType}",
            Body = $"Hi {sellerName}, {details}",
            IsHtml = true,
            TemplateName = "SellerNotification"
        });
        
        return Task.CompletedTask;
    }

    public Task SendAccountStatusChangeAsync(string email, string firstName, bool isSuspended, string? reason)
    {
        _sentEmails.Add(new SentEmail
        {
            To = email,
            Subject = isSuspended ? "Account Suspended" : "Account Reactivated",
            Body = isSuspended 
                ? $"Hi {firstName}, your account has been suspended. Reason: {reason ?? "Terms of Service violation"}"
                : $"Hi {firstName}, your account has been reactivated. Welcome back!",
            IsHtml = true,
            TemplateName = "AccountStatusChange"
        });
        
        return Task.CompletedTask;
    }

    public Task SendAccountDeletionConfirmationAsync(string email, string firstName)
    {
        _sentEmails.Add(new SentEmail
        {
            To = email,
            Subject = "Account Deletion Confirmation",
            Body = $"Hi {firstName}, your UPTRMS account has been successfully deleted. If you did not request this deletion, please contact our support team immediately.",
            IsHtml = true,
            TemplateName = "AccountDeletion"
        });
        
        return Task.CompletedTask;
    }

    public IReadOnlyList<SentEmail> GetSentEmails() => _sentEmails.AsReadOnly();

    public void ClearSentEmails() => _sentEmails.Clear();

    public class SentEmail
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; }
        public string? TemplateName { get; set; }
    }
}