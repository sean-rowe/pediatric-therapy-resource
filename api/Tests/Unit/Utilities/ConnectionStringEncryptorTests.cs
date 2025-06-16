using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using TherapyDocs.Api.Utilities;
using Xunit;
using System.IO;

namespace TherapyDocs.Api.Tests.Unit.Utilities;

public class ConnectionStringEncryptorTests
{
    #region Main Method Tests

    [Fact]
    public void Main_NoArguments_PrintsUsage()
    {
        // Arrange
        var args = Array.Empty<string>();
        var originalOut = Console.Out;
        using var sw = new StringWriter();
        Console.SetOut(sw);

        try
        {
            // Act
            ConnectionStringEncryptor.EncryptConnectionString(args);

            // Assert
            var output = sw.ToString();
            Assert.Contains("Usage: dotnet run", output);
            Assert.Contains("--project ConnectionStringEncryptor", output);
            Assert.Contains("<connection-string>", output);
            Assert.Contains("Example:", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void Main_WithConnectionString_EncryptsSuccessfully()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TherapyDocs;User Id=sa;Password=Test123!";
        var args = new[] { connectionString };
        var originalOut = Console.Out;
        using var sw = new StringWriter();
        Console.SetOut(sw);

        try
        {
            // Act
            ConnectionStringEncryptor.EncryptConnectionString(args);

            // Assert
            var output = sw.ToString();
            Assert.Contains("Original connection string:", output);
            Assert.Contains("Password=****", output); // Password should be masked
            Assert.DoesNotContain("Test123!", output); // Actual password should not appear
            Assert.Contains("Encrypted connection string", output);
            Assert.Contains("\"DefaultConnection\": \"encrypted:", output);
            Assert.Contains("To verify decryption:", output);
            Assert.Contains("Decryption successful: ✓", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void Main_EmptyConnectionString_StillEncrypts()
    {
        // Arrange
        var args = new[] { "" };
        var originalOut = Console.Out;
        using var sw = new StringWriter();
        Console.SetOut(sw);

        try
        {
            // Act
            ConnectionStringEncryptor.EncryptConnectionString(args);

            // Assert
            var output = sw.ToString();
            Assert.Contains("Original connection string:", output);
            Assert.Contains("Encrypted connection string", output);
            Assert.Contains("encrypted:", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    #endregion

    #region MaskPassword Tests

    [Fact]
    public void MaskPassword_StandardConnectionString_MasksPasswordCorrectly()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=Test;User Id=sa;Password=Secret123!;";
        var method = typeof(ConnectionStringEncryptor).GetMethod("MaskPassword",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var masked = (string?)method?.Invoke(null, new object[] { connectionString });

        // Assert
        Assert.Contains("Server=localhost", masked);
        Assert.Contains("Database=Test", masked);
        Assert.Contains("User Id=sa", masked);
        Assert.Contains("Password=****", masked);
        Assert.DoesNotContain("Secret123!", masked);
    }

    [Fact]
    public void MaskPassword_PwdInsteadOfPassword_MasksCorrectly()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=Test;User Id=sa;Pwd=Secret123!;";
        var method = typeof(ConnectionStringEncryptor).GetMethod("MaskPassword",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var masked = (string?)method?.Invoke(null, new object[] { connectionString });

        // Assert
        Assert.Contains("Pwd=****", masked);
        Assert.DoesNotContain("Secret123!", masked);
    }

    [Fact]
    public void MaskPassword_CaseInsensitive_MasksCorrectly()
    {
        // Arrange
        var connectionStrings = new[]
        {
            "Server=localhost;PASSWORD=Secret123!;",
            "Server=localhost;password=Secret123!;",
            "Server=localhost;PaSsWoRd=Secret123!;",
            "Server=localhost;PWD=Secret123!;",
            "Server=localhost;pwd=Secret123!;"
        };
        
        var method = typeof(ConnectionStringEncryptor).GetMethod("MaskPassword",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        foreach (var connectionString in connectionStrings)
        {
            // Act
            var masked = (string?)method?.Invoke(null, new object[] { connectionString });

            // Assert
            Assert.Contains("=****", masked);
            Assert.DoesNotContain("Secret123!", masked);
        }
    }

    [Fact]
    public void MaskPassword_NoPassword_ReturnsUnchanged()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=Test;User Id=sa;Integrated Security=true;";
        var method = typeof(ConnectionStringEncryptor).GetMethod("MaskPassword",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var masked = (string?)method?.Invoke(null, new object[] { connectionString });

        // Assert
        Assert.Equal(connectionString, masked);
    }

    [Fact]
    public void MaskPassword_MultiplePasswords_MasksAll()
    {
        // Arrange
        var connectionString = "Server=localhost;Password=Secret1;Database=Test;Pwd=Secret2;";
        var method = typeof(ConnectionStringEncryptor).GetMethod("MaskPassword",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var masked = (string?)method?.Invoke(null, new object[] { connectionString });

        // Assert
        Assert.Contains("Password=****", masked);
        Assert.Contains("Pwd=****", masked);
        Assert.DoesNotContain("Secret1", masked);
        Assert.DoesNotContain("Secret2", masked);
    }

    [Fact]
    public void MaskPassword_PasswordWithSpecialCharacters_MasksCorrectly()
    {
        // Arrange
        var connectionString = "Server=localhost;Password=P@$$w0rd!#$%^&*();";
        var method = typeof(ConnectionStringEncryptor).GetMethod("MaskPassword",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var masked = (string?)method?.Invoke(null, new object[] { connectionString });

        // Assert
        Assert.Contains("Password=****", masked);
        Assert.DoesNotContain("P@$$w0rd!#$%^&*()", masked);
    }

    [Fact]
    public void MaskPassword_EmptyPassword_MasksCorrectly()
    {
        // Arrange
        var connectionString = "Server=localhost;Password=;Database=Test;";
        var method = typeof(ConnectionStringEncryptor).GetMethod("MaskPassword",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var masked = (string?)method?.Invoke(null, new object[] { connectionString });

        // Assert
        Assert.Contains("Password=****", masked);
    }

    [Fact]
    public void MaskPassword_MalformedConnectionString_HandlesGracefully()
    {
        // Arrange
        var connectionStrings = new[]
        {
            "Password", // No equals sign
            "Password=", // No value
            ";;Password=Secret;;", // Multiple semicolons
            "Password=Secret=123", // Multiple equals signs
            " Password = Secret ", // Spaces around equals
        };
        
        var method = typeof(ConnectionStringEncryptor).GetMethod("MaskPassword",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        foreach (var connectionString in connectionStrings)
        {
            // Act & Assert - Should not throw
            var masked = (string?)method?.Invoke(null, new object[] { connectionString });
            Assert.NotNull(masked);
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Main_VeryLongConnectionString_HandlesCorrectly()
    {
        // Arrange
        var longPassword = new string('a', 1000);
        var connectionString = $"Server=localhost;Database=Test;Password={longPassword};";
        var args = new[] { connectionString };
        var originalOut = Console.Out;
        using var sw = new StringWriter();
        Console.SetOut(sw);

        try
        {
            // Act
            ConnectionStringEncryptor.EncryptConnectionString(args);

            // Assert
            var output = sw.ToString();
            Assert.Contains("Password=****", output);
            Assert.DoesNotContain(longPassword, output);
            Assert.Contains("encrypted:", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void Main_ConnectionStringWithQuotes_HandlesCorrectly()
    {
        // Arrange
        var connectionString = "Server=\"localhost\";Database='Test';Password=\"Secret'123\";";
        var args = new[] { connectionString };
        var originalOut = Console.Out;
        using var sw = new StringWriter();
        Console.SetOut(sw);

        try
        {
            // Act
            ConnectionStringEncryptor.EncryptConnectionString(args);

            // Assert
            var output = sw.ToString();
            Assert.Contains("Password=****", output);
            Assert.Contains("encrypted:", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void MaskPassword_NullConnectionString_HandlesGracefully()
    {
        // Arrange
        string? connectionString = null;
        var method = typeof(ConnectionStringEncryptor).GetMethod("MaskPassword",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act & Assert - Should handle null without throwing
        var masked = method?.Invoke(null, new object?[] { connectionString });
        // The actual behavior depends on implementation, but it shouldn't throw
    }

    #endregion
}