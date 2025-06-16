namespace TherapyDocs.Api.Interfaces;

public interface IEmailVerificationService
{
    Task SendVerificationEmailAsync(Guid userId, string email, string firstName);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> ResendVerificationEmailAsync(string email);
}