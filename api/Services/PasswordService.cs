using BCrypt.Net;

namespace TherapyDocs.Api.Services;

public class PasswordService : IPasswordService
{
    private static readonly HashSet<string> CommonPasswords = new()
    {
        "password", "123456", "123456789", "qwerty", "abc123", "password123",
        "admin", "letmein", "welcome", "monkey", "dragon", "master",
        "sunshine", "princess", "football", "baseball", "superman",
        "iloveyou", "trustno1", "hello", "freedom", "whatever"
    };

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public bool IsCommonPassword(string password)
    {
        return CommonPasswords.Contains(password.ToLowerInvariant());
    }
}