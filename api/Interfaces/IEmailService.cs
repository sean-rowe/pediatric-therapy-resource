using System.Threading.Tasks;

namespace TherapyDocs.Api.Interfaces;

public interface IEmailService
{
    Task<bool> SendVerificationEmailAsync(string email, string firstName, string verificationToken);
    Task<bool> SendWelcomeEmailAsync(string email, string firstName);
}