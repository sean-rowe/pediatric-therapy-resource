using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Security;

public class JwtSecurityTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<IAccountLockoutRepository> _mockLockoutRepository;
    private readonly Mock<IPasswordHistoryRepository> _mockPasswordHistoryRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<LoginService>> _mockLogger;
    private readonly LoginService _loginService;
    private readonly string _jwtKey = "super-secret-key-that-is-at-least-32-characters-long-for-security";

    public JwtSecurityTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockLockoutRepository = new Mock<IAccountLockoutRepository>();
        _mockPasswordHistoryRepository = new Mock<IPasswordHistoryRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<LoginService>>();

        // Setup JWT configuration
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(_jwtKey);
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TherapyDocs");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TherapyDocs");

        _loginService = new LoginService(
            _mockUserRepository.Object,
            _mockPasswordService.Object,
            _mockLockoutRepository.Object,
            _mockPasswordHistoryRepository.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_GeneratesValidJwtToken()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var user = CreateValidUser();
        SetupSuccessfulLogin(request, user);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        
        // Validate JWT token structure
        var tokenHandler = new JwtSecurityTokenHandler();
        Assert.True(tokenHandler.CanReadToken(result.Token));
        
        var token = tokenHandler.ReadJwtToken(result.Token);
        Assert.NotNull(token);
        Assert.NotEmpty(token.Claims);
    }

    [Fact]
    public async Task LoginAsync_GeneratedToken_ContainsRequiredClaims()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var user = CreateValidUser();
        SetupSuccessfulLogin(request, user);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token!);
        
        // Check required claims
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Name && c.Value == $"{user.FirstName} {user.LastName}");
        Assert.Contains(token.Claims, c => c.Type == "service_type" && c.Value == user.ServiceType);
        Assert.Contains(token.Claims, c => c.Type == "license_number" && c.Value == user.LicenseNumber);
        Assert.Contains(token.Claims, c => c.Type == "license_state" && c.Value == user.LicenseState);
    }

    [Fact]
    public async Task LoginAsync_GeneratedToken_HasCorrectExpiration()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var user = CreateValidUser();
        SetupSuccessfulLogin(request, user);
        var loginTime = DateTime.UtcNow;

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token!);
        
        // Token should expire in 8 hours (as configured in LoginService)
        var expectedExpiry = loginTime.AddHours(8);
        var tokenExpiry = token.ValidTo;
        
        // Allow 1 minute tolerance for test execution time
        Assert.True(Math.Abs((tokenExpiry - expectedExpiry).TotalMinutes) < 1,
            $"Expected expiry around {expectedExpiry}, but got {tokenExpiry}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-key")]
    [InlineData("short")]
    public void JwtTokenGeneration_InvalidKey_ShouldThrowException(string invalidKey)
    {
        // Arrange
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(invalidKey);
        
        // Act & Assert
        Assert.ThrowsAny<Exception>(() => new LoginService(
            _mockUserRepository.Object,
            _mockPasswordService.Object,
            _mockLockoutRepository.Object,
            _mockPasswordHistoryRepository.Object,
            _mockConfiguration.Object,
            _mockLogger.Object));
    }

    [Fact]
    public async Task JwtToken_DoesNotContainSensitiveInformation()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var user = CreateValidUser();
        SetupSuccessfulLogin(request, user);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token!);
        
        // Ensure sensitive information is NOT in the token
        Assert.DoesNotContain(token.Claims, c => c.Value.Contains(user.PasswordHash));
        Assert.DoesNotContain(token.Claims, c => c.Type.Contains("password"));
        Assert.DoesNotContain(token.Claims, c => c.Type.Contains("hash"));
        
        // Also check the raw token payload doesn't contain sensitive data
        var payload = token.Payload.SerializeToJson();
        Assert.DoesNotContain("password", payload.ToLower());
        Assert.DoesNotContain("hash", payload.ToLower());
    }

    [Fact]
    public async Task JwtToken_HasCorrectIssuerAndAudience()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var user = CreateValidUser();
        SetupSuccessfulLogin(request, user);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token!);
        
        Assert.Equal("TherapyDocs", token.Issuer);
        Assert.Contains("TherapyDocs", token.Audiences);
    }

    [Fact]
    public async Task JwtToken_IsProperlySignedWithHmacSha256()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var user = CreateValidUser();
        SetupSuccessfulLogin(request, user);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token!);
        
        Assert.Equal("HS256", token.Header.Alg);
        Assert.Equal("JWT", token.Header.Typ);
        
        // Verify token has a signature
        var tokenParts = result.Token!.Split('.');
        Assert.Equal(3, tokenParts.Length); // Header.Payload.Signature
        Assert.NotEmpty(tokenParts[2]); // Signature should not be empty
    }

    [Fact]
    public async Task JwtToken_DifferentUsers_GenerateDifferentTokens()
    {
        // Arrange
        var user1 = CreateValidUser();
        user1.Id = 1;
        user1.Email = "user1@test.com";
        
        var user2 = CreateValidUser();
        user2.Id = 2;
        user2.Email = "user2@test.com";

        var request1 = new LoginRequest { Email = user1.Email, Password = "ValidPassword123!" };
        var request2 = new LoginRequest { Email = user2.Email, Password = "ValidPassword123!" };

        SetupSuccessfulLogin(request1, user1);
        SetupSuccessfulLogin(request2, user2);

        // Act
        var result1 = await _loginService.LoginAsync(request1, "127.0.0.1", "TestAgent");
        var result2 = await _loginService.LoginAsync(request2, "127.0.0.1", "TestAgent");

        // Assert
        Assert.True(result1.Success);
        Assert.True(result2.Success);
        Assert.NotEqual(result1.Token, result2.Token);
        
        // Verify different user claims
        var tokenHandler = new JwtSecurityTokenHandler();
        var token1 = tokenHandler.ReadJwtToken(result1.Token!);
        var token2 = tokenHandler.ReadJwtToken(result2.Token!);
        
        var userId1 = token1.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var userId2 = token2.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        
        Assert.NotEqual(userId1, userId2);
    }

    [Fact]
    public async Task JwtToken_ConsecutiveLogins_GenerateDifferentTokens()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var user = CreateValidUser();
        SetupSuccessfulLogin(request, user);

        // Act - Login twice with same credentials
        var result1 = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");
        await Task.Delay(10); // Small delay to ensure different timestamps
        var result2 = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.True(result1.Success);
        Assert.True(result2.Success);
        Assert.NotEqual(result1.Token, result2.Token);
        
        // Both tokens should be valid but have different issue times
        var tokenHandler = new JwtSecurityTokenHandler();
        var token1 = tokenHandler.ReadJwtToken(result1.Token!);
        var token2 = tokenHandler.ReadJwtToken(result2.Token!);
        
        // Issue times should be different (even if slightly)
        Assert.NotEqual(token1.IssuedAt, token2.IssuedAt);
    }

    private void SetupSuccessfulLogin(LoginRequest request, User user)
    {
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 5 };
        var passwordRequirement = new PasswordChangeRequirement { ChangeRequired = false, DaysUntilExpiry = 30 };

        _mockLockoutRepository.Setup(r => r.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(s => s.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);
        _mockPasswordHistoryRepository.Setup(r => r.CheckPasswordChangeRequiredAsync(user.Id))
            .ReturnsAsync(passwordRequirement);
    }

    private User CreateValidUser()
    {
        return new User
        {
            Id = 1,
            Email = "test@test.com",
            PasswordHash = "$2a$12$dummy.hash.for.testing",
            FirstName = "Test",
            LastName = "User",
            ServiceType = "speech_therapy",
            Status = "active",
            EmailVerified = true,
            LicenseNumber = "12345",
            LicenseState = "CA"
        };
    }
}