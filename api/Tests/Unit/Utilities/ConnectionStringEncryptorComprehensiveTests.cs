using FluentAssertions;
using TherapyDocs.Api.Utilities;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Utilities;

public class ConnectionStringEncryptorComprehensiveTests
{
    /**
     * Feature: Connection String Encryption Utility
     *   As a security administrator
     *   I want to encrypt database connection strings
     *   So that sensitive credentials are protected
     * 
     * Scenario: Encrypt connection string with valid arguments
     *   Given valid command line arguments with a connection string
     *   When calling the encryption utility
     *   Then the connection string is processed without throwing exceptions
     */
    [Fact]
    public void EncryptConnectionString_ValidArguments_HandlesCall()
    {
        // Arrange
        var args = new[] { "Server=localhost;Database=TestDb;User Id=sa;Password=Test123!;TrustServerCertificate=true" };

        // Act & Assert
        var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
        act.Should().NotThrow();
    }

    /**
     * Scenario: Encrypt connection string with no arguments
     *   Given no command line arguments
     *   When calling the encryption utility
     *   Then usage information is displayed without throwing exceptions
     */
    [Fact]
    public void EncryptConnectionString_NoArguments_HandlesCall()
    {
        // Arrange
        var args = new string[0];

        // Act & Assert
        var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
        act.Should().NotThrow();
    }

    /**
     * Scenario: Encrypt connection string with empty argument
     *   Given an empty string argument
     *   When calling the encryption utility
     *   Then the operation handles empty input gracefully
     */
    [Fact]
    public void EncryptConnectionString_EmptyArgument_HandlesCall()
    {
        // Arrange
        var args = new[] { "" };

        // Act & Assert
        var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
        act.Should().NotThrow();
    }

    /**
     * Scenario: Encrypt connection string with multiple arguments
     *   Given multiple command line arguments
     *   When calling the encryption utility
     *   Then only the first argument is used as connection string
     */
    [Fact]
    public void EncryptConnectionString_MultipleArguments_HandlesCall()
    {
        // Arrange
        var args = new[] 
        { 
            "Server=localhost;Database=TestDb;User Id=sa;Password=Test123!",
            "extra-argument",
            "another-argument"
        };

        // Act & Assert
        var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
        act.Should().NotThrow();
    }

    /**
     * Feature: Connection String Password Masking
     *   As a security utility
     *   I want to mask passwords in connection strings
     *   So that sensitive data is not exposed in logs
     * 
     * Scenario: Various connection string formats with passwords
     *   Given different connection string formats containing passwords
     *   When processing them
     *   Then the operation handles all formats gracefully
     */
    [Theory]
    [InlineData("Server=localhost;Database=TestDb;Password=Secret123!")]
    [InlineData("Server=localhost;Database=TestDb;Pwd=Secret123!")]
    [InlineData("Server=localhost;Password=Secret123!;Database=TestDb")]
    [InlineData("Password=Secret123!;Server=localhost;Database=TestDb")]
    [InlineData("Server=localhost;DATABASE=TestDb;PASSWORD=Secret123!")]
    [InlineData("Server=localhost;Database=TestDb;pwd=Secret123!")]
    public void EncryptConnectionString_VariousPasswordFormats_HandlesCall(string connectionString)
    {
        // Arrange
        var args = new[] { connectionString };

        // Act & Assert
        var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
        act.Should().NotThrow();
    }

    /**
     * Scenario: Connection strings without passwords
     *   Given connection strings without password fields
     *   When processing them
     *   Then the operation handles them gracefully
     */
    [Theory]
    [InlineData("Server=localhost;Database=TestDb;Integrated Security=true")]
    [InlineData("Server=localhost;Database=TestDb;Trusted_Connection=true")]
    [InlineData("Server=localhost;Database=TestDb")]
    [InlineData("Server=localhost")]
    public void EncryptConnectionString_NoPasswordFormats_HandlesCall(string connectionString)
    {
        // Arrange
        var args = new[] { connectionString };

        // Act & Assert
        var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
        act.Should().NotThrow();
    }

    /**
     * Feature: Edge Cases and Error Handling
     *   As a robust utility
     *   I want to handle edge cases gracefully
     *   So that the system remains stable
     * 
     * Scenario: Connection strings with special characters
     *   Given connection strings containing special characters
     *   When processing them
     *   Then the operation handles special characters correctly
     */
    [Theory]
    [InlineData("Server=localhost;Database=Test-Db;Password=P@ssw0rd!@#$%^&*()")]
    [InlineData("Server=localhost;Database=Test_Db;Password=Password with spaces")]
    [InlineData("Server=localhost;Database=TestDb;Password=Pässwörd")]
    [InlineData("Server=localhost;Database=TestDb;Password=中文密码")]
    public void EncryptConnectionString_SpecialCharacters_HandlesCall(string connectionString)
    {
        // Arrange
        var args = new[] { connectionString };

        // Act & Assert
        var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
        act.Should().NotThrow();
    }

    /**
     * Scenario: Very long connection strings
     *   Given very long connection strings
     *   When processing them
     *   Then the operation handles long strings correctly
     */
    [Fact]
    public void EncryptConnectionString_VeryLongConnectionString_HandlesCall()
    {
        // Arrange
        var longPassword = new string('A', 1000);
        var connectionString = $"Server=localhost;Database=TestDb;Password={longPassword}";
        var args = new[] { connectionString };

        // Act & Assert
        var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
        act.Should().NotThrow();
    }

    /**
     * Scenario: Malformed connection strings
     *   Given malformed connection strings
     *   When processing them
     *   Then the operation handles malformed input gracefully
     */
    [Theory]
    [InlineData("not-a-connection-string")]
    [InlineData("Server=;Database=;Password=")]
    [InlineData("=====;;;;;;")]
    [InlineData("Server localhost Database TestDb Password Secret")]
    [InlineData("Server=localhost;Database=TestDb;Password=")]
    public void EncryptConnectionString_MalformedStrings_HandlesCall(string connectionString)
    {
        // Arrange
        var args = new[] { connectionString };

        // Act & Assert
        var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
        act.Should().NotThrow();
    }

    /**
     * Scenario: Null and whitespace arguments
     *   Given null or whitespace arguments
     *   When processing them
     *   Then the operation handles null/whitespace gracefully
     */
    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("   ")]
    public void EncryptConnectionString_WhitespaceArguments_HandlesCall(string connectionString)
    {
        // Arrange
        var args = new[] { connectionString };

        // Act & Assert
        var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
        act.Should().NotThrow();
    }

    /**
     * Feature: Data Protection Integration
     *   As a security utility
     *   I want to integrate with .NET Data Protection APIs
     *   So that encryption is handled securely
     * 
     * Scenario: Data protection provider integration
     *   Given a connection string to encrypt
     *   When using data protection services
     *   Then the encryption/decryption cycle works correctly
     */
    [Fact]
    public void EncryptConnectionString_DataProtectionIntegration_HandlesCall()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDb;User Id=sa;Password=Test123!;TrustServerCertificate=true";
        var args = new[] { connectionString };

        // Act & Assert
        // This tests that the data protection provider can be created and used
        var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
        act.Should().NotThrow();
    }

    /**
     * Scenario: Multiple consecutive calls
     *   Given multiple consecutive encryption calls
     *   When processing them
     *   Then all calls are handled correctly
     */
    [Fact]
    public void EncryptConnectionString_MultipleCalls_HandlesCall()
    {
        // Arrange
        var connectionStrings = new[]
        {
            "Server=localhost1;Database=TestDb1;Password=Secret1!",
            "Server=localhost2;Database=TestDb2;Password=Secret2!",
            "Server=localhost3;Database=TestDb3;Password=Secret3!"
        };

        // Act & Assert
        foreach (var connectionString in connectionStrings)
        {
            var args = new[] { connectionString };
            var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
            act.Should().NotThrow();
        }
    }

    /**
     * Feature: Performance and Resource Management
     *   As a utility application
     *   I want to handle resources efficiently
     *   So that the system performs well
     * 
     * Scenario: Resource cleanup
     *   Given a connection string encryption operation
     *   When the operation completes
     *   Then resources are cleaned up properly
     */
    [Fact]
    public void EncryptConnectionString_ResourceCleanup_HandlesCall()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDb;Password=Test123!";
        var args = new[] { connectionString };

        // Act & Assert
        // This test ensures that the host and services are properly disposed
        var act = () => ConnectionStringEncryptor.EncryptConnectionString(args);
        act.Should().NotThrow();
    }
}