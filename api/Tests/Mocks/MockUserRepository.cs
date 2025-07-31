using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Repositories;

namespace UPTRMS.Api.Tests.Mocks;

public class MockUserRepository : IUserRepository
{
    public MockUserRepository()
    {
    }

    public static void ClearState()
    {
        SharedMockUserStore.Clear();
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        await Task.Delay(1); // Simulate async
        return SharedMockUserStore.GetUserById(userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await Task.Delay(1); // Simulate async
        return SharedMockUserStore.GetUserByEmail(email);
    }

    public async Task<bool> CreateAsync(User user)
    {
        await Task.Delay(1); // Simulate async
        
        if (SharedMockUserStore.GetUserById(user.UserId) != null || SharedMockUserStore.UserExistsByEmail(user.Email))
            return false;
        
        SharedMockUserStore.AddUser(user);
        return true;
    }

    public async Task UpdateAsync(User user)
    {
        await Task.Delay(1); // Simulate async
        
        if (SharedMockUserStore.GetUserById(user.UserId) == null)
            throw new InvalidOperationException("User not found");
        
        SharedMockUserStore.UpdateUser(user);
    }

    public async Task DeleteAsync(User entity)
    {
        await Task.Delay(1); // Simulate async
        
        if (SharedMockUserStore.GetUserById(entity.UserId) == null)
            throw new InvalidOperationException("User not found");
        
        SharedMockUserStore.RemoveUser(entity.UserId);
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        await Task.Delay(1); // Simulate async
        
        return SharedMockUserStore.RemoveUser(userId);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        await Task.Delay(1); // Simulate async
        return SharedMockUserStore.UserExistsByEmail(email);
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize)
    {
        await Task.Delay(1); // Simulate async
        
        var query = SharedMockUserStore.GetAllUsers().AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(u => 
                u.Email.ToLower().Contains(searchTerm) ||
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.LicenseNumber.ToLower().Contains(searchTerm));
        }
        
        var totalCount = query.Count();
        var users = query
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        return (users, totalCount);
    }

    public async Task<bool> UpdateLastLoginAsync(Guid userId)
    {
        await Task.Delay(1); // Simulate async
        
        var user = SharedMockUserStore.GetUserById(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            return true;
        }
        
        return false;
    }

    // IUserRepository specific methods
    public async Task<User?> GetByIdWithOrganizationAsync(Guid userId)
    {
        await Task.Delay(1); // Simulate async
        // For testing, just return the user without organization loading
        return await GetByIdAsync(userId);
    }

    public async Task<IEnumerable<User>> GetByOrganizationAsync(Guid organizationId)
    {
        await Task.Delay(1); // Simulate async
        // For testing, return empty list as we don't have organization support
        return Enumerable.Empty<User>();
    }

    public async Task<IEnumerable<User>> GetSellersAsync(bool approvedOnly = true)
    {
        await Task.Delay(1); // Simulate async
        // For testing, return users marked as approved sellers
        if (approvedOnly)
            return SharedMockUserStore.GetAllUsers().Where(u => u.IsSellerApproved);
        else
            return SharedMockUserStore.GetAllUsers().Where(u => u.IsSellerApproved || u.SellerProfile != null);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null)
    {
        await Task.Delay(1); // Simulate async
        
        var existingUser = SharedMockUserStore.GetUserByEmail(email);
        if (existingUser != null)
        {
            return excludeUserId.HasValue && existingUser.UserId == excludeUserId.Value;
        }
        
        return true;
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int skip = 0, int take = 20)
    {
        await Task.Delay(1); // Simulate async
        
        var query = SharedMockUserStore.GetAllUsers().AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(u => 
                u.Email.ToLower().Contains(searchTerm) ||
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm));
        }
        
        return query.Skip(skip).Take(take).ToList();
    }

    // IRepository<User> methods
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        await Task.Delay(1); // Simulate async
        return SharedMockUserStore.GetAllUsers().ToList();
    }

    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
    {
        await Task.Delay(1); // Simulate async
        var compiledPredicate = predicate.Compile();
        return SharedMockUserStore.GetAllUsers().Where(compiledPredicate).ToList();
    }

    public async Task<User> AddAsync(User entity)
    {
        await Task.Delay(1); // Simulate async
        
        if (SharedMockUserStore.GetUserById(entity.UserId) != null || SharedMockUserStore.UserExistsByEmail(entity.Email))
            throw new InvalidOperationException("User already exists");
        
        SharedMockUserStore.AddUser(entity);
        return entity;
    }

    public async Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate)
    {
        await Task.Delay(1); // Simulate async
        var compiledPredicate = predicate.Compile();
        return SharedMockUserStore.GetAllUsers().Any(compiledPredicate);
    }

    public async Task<int> CountAsync(Expression<Func<User, bool>>? predicate = null)
    {
        await Task.Delay(1); // Simulate async
        
        if (predicate == null)
            return SharedMockUserStore.GetAllUsers().Count();
        
        var compiledPredicate = predicate.Compile();
        return SharedMockUserStore.GetAllUsers().Count(compiledPredicate);
    }

    public IQueryable<User> Query()
    {
        return SharedMockUserStore.GetAllUsers().AsQueryable();
    }
}