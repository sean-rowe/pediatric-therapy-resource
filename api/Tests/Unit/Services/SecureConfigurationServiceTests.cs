using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Cryptography;
using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class SecureConfigurationServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IDataProtectionProvider> _mockDataProtectionProvider;
    private readonly Mock<IDataProtector> _mockDataProtector;
    private readonly Mock<ILogger<SecureConfigurationService>> _mockLogger;
    private readonly SecureConfigurationService _service;

    public SecureConfigurationServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
        _mockDataProtector = new Mock<IDataProtector>();
        _mockLogger = new Mock<ILogger<SecureConfigurationService>>();

        _mockDataProtectionProvider
            .Setup(x => x.CreateProtector("ConnectionStrings"))
            .Returns(_mockDataProtector.Object);

        _service = new SecureConfigurationService(
            _mockConfiguration.Object,
            _mockDataProtectionProvider.Object,
            _mockLogger.Object);
    }

    #region GetConnectionString Tests

    [Fact]
    public void GetConnectionString_PlainTextConnectionString_ReturnsAsIs()
    {
        // Arrange
        var connectionName = "DefaultConnection";
        var plainConnectionString = "Server=localhost;Database=TherapyDocs;User Id=sa;Password=Test123!";

        _mockConfiguration
            .Setup(x => x.GetConnectionString(connectionName))
            .Returns(plainConnectionString);

        // Act
        var result = _service.GetConnectionString(connectionName);

        // Assert
        Assert.Equal(plainConnectionString, result);
        _mockDataProtector.Verify(x => x.Unprotect(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void GetConnectionString_EncryptedConnectionString_DecryptsSuccessfully()
    {
        // Arrange
        var connectionName = "DefaultConnection";
        var encryptedValue = "encryptedBase64String";
        var encryptedConnectionString = $"encrypted:{encryptedValue}";
        var decryptedConnectionString = "Server=localhost;Database=TherapyDocs;User Id=sa;Password=Test123!";

        _mockConfiguration
            .Setup(x => x.GetConnectionString(connectionName))
            .Returns(encryptedConnectionString);

        _mockDataProtector
            .Setup(x => x.Unprotect(encryptedValue))
            .Returns(decryptedConnectionString);

        // Act
        var result = _service.GetConnectionString(connectionName);

        // Assert
        Assert.Equal(decryptedConnectionString, result);
        _mockDataProtector.Verify(x => x.Unprotect(encryptedValue), Times.Once);
    }

    [Fact]
    public void GetConnectionString_ConnectionStringNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var connectionName = "NonExistentConnection";

        _mockConfiguration
            .Setup(x => x.GetConnectionString(connectionName))
            .Returns((string?)null);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            _service.GetConnectionString(connectionName));
        
        Assert.Equal($"Connection string '{connectionName}' not found", exception.Message);
    }

    [Fact]
    public void GetConnectionString_EmptyConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var connectionName = "EmptyConnection";

        _mockConfiguration
            .Setup(x => x.GetConnectionString(connectionName))
            .Returns(string.Empty);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            _service.GetConnectionString(connectionName));
        
        Assert.Equal($"Connection string '{connectionName}' not found", exception.Message);
    }

    [Fact]
    public void GetConnectionString_DecryptionFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var connectionName = "DefaultConnection";
        var encryptedValue = "corruptedEncryptedValue";
        var encryptedConnectionString = $"encrypted:{encryptedValue}";

        _mockConfiguration
            .Setup(x => x.GetConnectionString(connectionName))
            .Returns(encryptedConnectionString);

        _mockDataProtector
            .Setup(x => x.Unprotect(encryptedValue))
            .Throws(new CryptographicException("Invalid data"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            _service.GetConnectionString(connectionName));
        
        Assert.Equal($"Failed to decrypt connection string '{connectionName}'", exception.Message);
        Assert.IsType<CryptographicException>(exception.InnerException);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to decrypt connection string '{connectionName}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("encrypted:")]
    [InlineData("encrypted: ")]
    [InlineData("encrypted:\t")]
    public void GetConnectionString_EmptyEncryptedValue_HandlesCorrectly(string encryptedConnectionString)
    {
        // Arrange
        var connectionName = "DefaultConnection";
        var expectedEncryptedPart = encryptedConnectionString.Substring("encrypted:".Length);

        _mockConfiguration
            .Setup(x => x.GetConnectionString(connectionName))
            .Returns(encryptedConnectionString);

        _mockDataProtector
            .Setup(x => x.Unprotect(expectedEncryptedPart))
            .Returns("decrypted-value");

        // Act
        var result = _service.GetConnectionString(connectionName);

        // Assert
        Assert.Equal("decrypted-value", result);
        _mockDataProtector.Verify(x => x.Unprotect(expectedEncryptedPart), Times.Once);
    }

    #endregion

    #region EncryptConnectionString Tests

    [Fact]
    public void EncryptConnectionString_ValidConnectionString_ReturnsEncryptedFormat()
    {
        // Arrange
        var plainConnectionString = "Server=localhost;Database=TherapyDocs;User Id=sa;Password=Test123!";
        var encryptedValue = "encryptedBase64String";

        _mockDataProtector
            .Setup(x => x.Protect(plainConnectionString))
            .Returns(encryptedValue);

        // Act
        var result = _service.EncryptConnectionString(plainConnectionString);

        // Assert
        Assert.Equal($"encrypted:{encryptedValue}", result);
        _mockDataProtector.Verify(x => x.Protect(plainConnectionString), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void EncryptConnectionString_InvalidConnectionString_StillEncrypts(string? connectionString)
    {
        // Arrange
        var encryptedValue = "encryptedEmptyValue";

        _mockDataProtector
            .Setup(x => x.Protect(It.IsAny<string>()))
            .Returns(encryptedValue);

        // Act
        var result = _service.EncryptConnectionString(connectionString!);

        // Assert
        Assert.Equal($"encrypted:{encryptedValue}", result);
    }

    [Fact]
    public void EncryptConnectionString_EncryptionThrows_PropagatesException()
    {
        // Arrange
        var plainConnectionString = "Server=localhost;Database=TherapyDocs;User Id=sa;Password=Test123!";

        _mockDataProtector
            .Setup(x => x.Protect(plainConnectionString))
            .Throws(new CryptographicException("Encryption failed"));

        // Act & Assert
        Assert.Throws<CryptographicException>(() => 
            _service.EncryptConnectionString(plainConnectionString));
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("Encrypted:value")] // Capital E
    [InlineData("ENCRYPTED:value")] // All caps
    [InlineData("eNcRyPtEd:value")] // Mixed case
    public void GetConnectionString_CaseSensitivePrefix_TreatsAsPlainText(string connectionString)
    {
        // Arrange
        var connectionName = "DefaultConnection";

        _mockConfiguration
            .Setup(x => x.GetConnectionString(connectionName))
            .Returns(connectionString);

        // Act
        var result = _service.GetConnectionString(connectionName);

        // Assert
        Assert.Equal(connectionString, result);
        _mockDataProtector.Verify(x => x.Unprotect(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void GetConnectionString_ConnectionStringWithEncryptedInMiddle_TreatsAsPlainText()
    {
        // Arrange
        var connectionName = "DefaultConnection";
        var connectionString = "Server=localhost;encrypted:Database=TherapyDocs";

        _mockConfiguration
            .Setup(x => x.GetConnectionString(connectionName))
            .Returns(connectionString);

        // Act
        var result = _service.GetConnectionString(connectionName);

        // Assert
        Assert.Equal(connectionString, result);
        _mockDataProtector.Verify(x => x.Unprotect(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void GetConnectionString_MultipleCallsSameConnection_CachesNothing()
    {
        // Arrange
        var connectionName = "DefaultConnection";
        var plainConnectionString = "Server=localhost;Database=TherapyDocs;User Id=sa;Password=Test123!";

        _mockConfiguration
            .Setup(x => x.GetConnectionString(connectionName))
            .Returns(plainConnectionString);

        // Act
        var result1 = _service.GetConnectionString(connectionName);
        var result2 = _service.GetConnectionString(connectionName);
        var result3 = _service.GetConnectionString(connectionName);

        // Assert
        Assert.Equal(plainConnectionString, result1);
        Assert.Equal(plainConnectionString, result2);
        Assert.Equal(plainConnectionString, result3);
        
        // Verify configuration is called each time (no caching)
        _mockConfiguration.Verify(x => x.GetConnectionString(connectionName), Times.Exactly(3));
    }

    #endregion

    #region SecureConfigurationExtensions Tests

    [Fact]
    public void AddSecureConfiguration_RegistersServicesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddSecureConfiguration();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var secureConfigService = serviceProvider.GetService<ISecureConfigurationService>();
        
        Assert.NotNull(secureConfigService);
        Assert.IsType<SecureConfigurationService>(secureConfigService);
        
        // Verify DataProtection is configured
        var dataProtectionProvider = serviceProvider.GetService<IDataProtectionProvider>();
        Assert.NotNull(dataProtectionProvider);
    }

    #endregion
}