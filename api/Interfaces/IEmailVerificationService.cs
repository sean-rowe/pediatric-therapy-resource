namespace TherapyDocs.Api.Interfaces;

public interface IEmailVerificationService
{
    Task SendVerificationEmailAsync(int userId, string email, string firstName);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> ResendVerificationEmailAsync(string email);
}