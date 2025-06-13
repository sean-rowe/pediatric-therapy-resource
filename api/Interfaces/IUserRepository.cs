using TherapyDocs.Api.Models;

namespace TherapyDocs.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> LicenseExistsAsync(string licenseNumber, string licenseState);
    Task<Guid> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task<bool> VerifyEmailAsync(Guid userId);
}