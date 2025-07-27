using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithOrganizationAsync(Guid userId);
    Task<IEnumerable<User>> GetByOrganizationAsync(Guid organizationId);
    Task<IEnumerable<User>> GetSellersAsync(bool approvedOnly = true);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int skip = 0, int take = 20);
}