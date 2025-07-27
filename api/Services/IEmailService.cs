namespace UPTRMS.Api.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string firstName, string token);
    Task SendPasswordResetEmailAsync(string email, string firstName, string token);
    Task SendWelcomeEmailAsync(string email, string firstName);
    Task SendInvitationEmailAsync(string email, string inviterName, string organizationName, string inviteCode);
    Task SendResourceSharedEmailAsync(string email, string recipientName, string senderName, string resourceTitle, string accessLink);
    Task SendPaymentReceiptAsync(string email, string firstName, decimal amount, string description);
    Task SendSellerNotificationAsync(string email, string sellerName, string notificationType, string details);
}