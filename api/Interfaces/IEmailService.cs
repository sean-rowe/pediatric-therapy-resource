namespace TherapyDocs.Api.Services;

public interface IEmailService
{
    Task<bool> SendVerificationEmailAsync(string email, string firstName, string verificationToken);
    Task<bool> SendWelcomeEmailAsync(string email, string firstName);
}