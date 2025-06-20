using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using TherapyDocs.Api.Interfaces;

namespace TherapyDocs.Api.Services;

public class PasswordService : IPasswordService
{
    private readonly IHaveIBeenPwnedService _hibpService;
    private readonly ILogger<PasswordService> _logger;
    
    // NIST 800-63B guidelines - check against commonly used passwords
    private static readonly HashSet<string> CommonPasswords = new()
    {
        "password", "123456", "123456789", "qwerty", "abc123", "password123",
        "admin", "letmein", "welcome", "monkey", "dragon", "master",
        "sunshine", "princess", "football", "baseball", "superman",
        "iloveyou", "trustno1", "hello", "freedom", "whatever",
        "password1", "123123", "111111", "1234567", "12345678",
        "qwerty123", "1q2w3e4r", "admin123", "welcome123", "password1234",
        "passw0rd", "p@ssw0rd", "p@ssword", "pa55word", "pa55w0rd"
    };

    // Additional patterns that indicate weak passwords
    private static readonly Regex[] WeakPatterns = new[]
    {
        new Regex(@"^(.)\1+$", RegexOptions.Compiled), // All same character
        new Regex(@"^(01|12|23|34|45|56|67|78|89|98|87|76|65|54|43|32|21|10)+$", RegexOptions.Compiled), // Sequential numbers
        new Regex(@"^(abc|bcd|cde|def|efg|fgh|ghi|hij|ijk|jkl|klm|lmn|mno|nop|opq|pqr|qrs|rst|stu|tuv|uvw|vwx|wxy|xyz)+$", RegexOptions.IgnoreCase | RegexOptions.Compiled), // Sequential letters
        new Regex(@"^(qwerty|asdf|zxcv|qazwsx|zaq1|xsw2|cde3|vfr4|bgt5|nhy6|mju7|,ki8|.lo9|/;p0)+", RegexOptions.IgnoreCase | RegexOptions.Compiled) // Keyboard patterns
    };

    public PasswordService(IHaveIBeenPwnedService hibpService, ILogger<PasswordService> logger)
    {
        _hibpService = hibpService;
        _logger = logger;
    }

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
        // Check against local common passwords list
        if (CommonPasswords.Contains(password.ToLowerInvariant()))
        {
            _logger.LogInformation("Password rejected - found in common passwords list");
            return true;
        }

        // Check for weak patterns
        foreach (var pattern in WeakPatterns)
        {
            if (pattern.IsMatch(password))
            {
                _logger.LogInformation("Password rejected - matches weak pattern");
                return true;
            }
        }

        // Check against HIBP database asynchronously
        // Note: This is called from sync context, so we use GetAwaiter().GetResult()
        // In production, consider making IsCommonPassword async
        try
        {
            var isPwned = _hibpService.IsPasswordPwnedAsync(password).GetAwaiter().GetResult();
            if (isPwned)
            {
                _logger.LogInformation("Password rejected - found in breach database");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not check password against HIBP, allowing password");
            
            // Fail open - if HIBP is down, don't block users
        }

        return false;
    }
}