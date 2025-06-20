using TherapyDocs.Api.Models;
using TherapyDocs.Api.Models.DTOs;

namespace TherapyDocs.Api.Interfaces;

public interface IPasswordHistoryRepository
{
    Task<bool> IsPasswordReusedAsync(Guid userId, string passwordHash);
    Task AddPasswordToHistoryAsync(Guid userId, string passwordHash);
    Task<PasswordChangeRequirement> CheckPasswordChangeRequiredAsync(Guid userId);
}