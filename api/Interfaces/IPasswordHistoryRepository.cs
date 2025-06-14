using TherapyDocs.Api.Models.DTOs;

namespace TherapyDocs.Api.Interfaces;

public interface IPasswordHistoryRepository
{
    Task<bool> IsPasswordReusedAsync(int userId, string passwordHash);
    Task AddPasswordToHistoryAsync(int userId, string passwordHash);
    Task<PasswordChangeRequirement> CheckPasswordChangeRequiredAsync(int userId);
}