using System;
using System.Collections.Generic;
using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Tests.Mocks;

/// <summary>
/// Shared user store to ensure consistency between MockAuthenticationService and MockUserRepository
/// </summary>
public static class SharedMockUserStore
{
    private static readonly Dictionary<Guid, User> _users = new();
    private static readonly Dictionary<string, User> _usersByEmail = new();
    private static readonly object _lock = new();

    static SharedMockUserStore()
    {
        SeedTestData();
    }

    private static void SeedTestData()
    {
        // Add admin user
        var adminUser = new User
        {
            UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "admin@uptrms.com",
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            LicenseNumber = "ADMIN123",
            Languages = new List<string> { "English" },
            Specialties = new List<string> { "Administration" },
            CreatedAt = DateTime.UtcNow.AddDays(-90),
            IsActive = true,
            EmailVerified = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!")
        };
        
        AddUser(adminUser);

        // Add regular test users
        for (int i = 1; i <= 50; i++)
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = $"user{i}@clinic.com",
                FirstName = $"User{i}",
                LastName = "Test",
                Role = UserRole.Therapist,
                LicenseNumber = $"LIC{i:D5}",
                Languages = new List<string> { "English" },
                Specialties = new List<string> { "Pediatrics" },
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                IsActive = true,
                EmailVerified = i % 5 != 0, // Some unverified users
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!")
            };
            
            AddUser(user);
        }
    }

    public static void AddUser(User user)
    {
        lock (_lock)
        {
            _users[user.UserId] = user;
            _usersByEmail[user.Email] = user;
        }
    }

    public static User? GetUserById(Guid userId)
    {
        lock (_lock)
        {
            return _users.TryGetValue(userId, out var user) ? user : null;
        }
    }

    public static User? GetUserByEmail(string email)
    {
        lock (_lock)
        {
            return _usersByEmail.TryGetValue(email, out var user) ? user : null;
        }
    }

    public static void UpdateUser(User user)
    {
        lock (_lock)
        {
            if (_users.ContainsKey(user.UserId))
            {
                var oldEmail = _users[user.UserId].Email;
                if (oldEmail != user.Email)
                {
                    _usersByEmail.Remove(oldEmail);
                    _usersByEmail[user.Email] = user;
                }
                _users[user.UserId] = user;
            }
        }
    }

    public static bool RemoveUser(Guid userId)
    {
        lock (_lock)
        {
            if (_users.TryGetValue(userId, out var user))
            {
                _users.Remove(userId);
                _usersByEmail.Remove(user.Email);
                return true;
            }
            return false;
        }
    }

    public static IEnumerable<User> GetAllUsers()
    {
        lock (_lock)
        {
            return new List<User>(_users.Values);
        }
    }

    public static bool UserExistsByEmail(string email)
    {
        lock (_lock)
        {
            return _usersByEmail.ContainsKey(email);
        }
    }

    public static void Clear()
    {
        lock (_lock)
        {
            _users.Clear();
            _usersByEmail.Clear();
            SeedTestData();
        }
    }
}