using TherapyDocs.Api.Models;

namespace TherapyDocs.Api.Repositories;

public interface IEmailVerificationRepository
{
    Task<string> CreateVerificationTokenAsync(Guid userId);
    Task<EmailVerificationToken?> GetTokenAsync(string token);
    Task<bool> MarkTokenUsedAsync(string token);
    Task<bool> HasValidTokenAsync(Guid userId);
}