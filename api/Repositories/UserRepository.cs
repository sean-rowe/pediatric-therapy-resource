using Microsoft.EntityFrameworkCore;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<User?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public override async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _dbSet
            .Where(u => !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdWithOrganizationAsync(Guid userId)
    {
        return await _dbSet
            .Where(u => !u.IsDeleted)
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<IEnumerable<User>> GetByOrganizationAsync(Guid organizationId)
    {
        return await _dbSet
            .Where(u => !u.IsDeleted && u.OrganizationId == organizationId)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetSellersAsync(bool approvedOnly = true)
    {
        var query = _dbSet.Where(u => !u.IsDeleted);

        if (approvedOnly)
        {
            query = query.Where(u => u.IsSellerApproved);
        }
        else
        {
            query = query.Where(u => u.SellerProfile != null);
        }

        return await query
            .Include(u => u.SellerProfile)
            .OrderByDescending(u => u.SellerProfile!.TotalSales)
            .ToListAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null)
    {
        var query = _dbSet.Where(u => !u.IsDeleted && u.Email == email);

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.UserId != excludeUserId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int skip = 0, int take = 20)
    {
        var loweredSearch = searchTerm.ToLower();

        return await _dbSet
            .Where(u => !u.IsDeleted && (
                u.Email.ToLower().Contains(loweredSearch) ||
                u.FirstName.ToLower().Contains(loweredSearch) ||
                u.LastName.ToLower().Contains(loweredSearch) ||
                (u.LicenseNumber != null && u.LicenseNumber.ToLower().Contains(loweredSearch))))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}