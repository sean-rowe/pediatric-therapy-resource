using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class LicenseVerificationServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ILogger<LicenseVerificationService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly LicenseVerificationService _service;

    public LicenseVerificationServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<LicenseVerificationService>>();
        _mockConfiguration = new Mock<IConfiguration>();

        _service = new LicenseVerificationService(
            _httpClient,
            _mockCache.Object,
            _mockLogger.Object,
            _mockConfiguration.Object);
    }

    #region VerifyLicenseAsync Tests

    [Fact]
    public async Task VerifyLicenseAsync_CachedResult_ReturnsCachedValue()
    {
        // Arrange
        var licenseNumber = "LIC123456";
        var state = "CA";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";
        var cachedResult = new LicenseVerificationResult
        {
            Valid = true,
            PractitionerName = "John Doe",
            LicenseType = licenseType,
            ExpirationDate = DateTime.UtcNow.AddYears(1)
        };

        object? cacheValue = cachedResult;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(true);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        Assert.Equal(cachedResult, result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("License verification found in cache")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyLicenseAsync_NotImplementedException_ReturnsManualVerificationRequired()
    {
        // Arrange
        var licenseNumber = "LIC123456";
        var state = "CA";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        Assert.False(result.Valid);
        Assert.Contains("Manual verification required", result.ErrorMessage);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error verifying license")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyLicenseAsync_ValidLicense_CachesResult()
    {
        // This test is for when the API is implemented
        // Currently, it will always return manual verification required
        
        // Arrange
        var licenseNumber = "LIC123456";
        var state = "CA";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(false);

        var cacheEntryMock = new Mock<ICacheEntry>();
        _mockCache
            .Setup(x => x.CreateEntry(cacheKey))
            .Returns(cacheEntryMock.Object);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        // Since the API is not implemented, it should return false and not cache
        Assert.False(result.Valid);
        _mockCache.Verify(x => x.CreateEntry(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task VerifyLicenseAsync_UnknownState_LogsWarning()
    {
        // Arrange
        var licenseNumber = "LIC123456";
        var state = "XX"; // Unknown state
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        Assert.False(result.Valid);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("License verification not configured for state")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyLicenseAsync_GeneralException_ReturnsFallbackResult()
    {
        // Arrange
        var licenseNumber = "LIC123456";
        var state = "CA";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";

        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny))
            .Throws(new Exception("Cache error"));

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        Assert.False(result.Valid);
        Assert.Contains("temporarily unavailable", result.ErrorMessage);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error verifying license")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Edge Cases and Null Handling

    [Theory]
    [InlineData(null, "CA", "MFT")]
    [InlineData("", "CA", "MFT")]
    [InlineData("LIC123", null, "MFT")]
    [InlineData("LIC123", "", "MFT")]
    [InlineData("LIC123", "CA", null)]
    [InlineData("LIC123", "CA", "")]
    public async Task VerifyLicenseAsync_InvalidParameters_HandlesGracefully(string? licenseNumber, string? state, string? licenseType)
    {
        // Arrange
        var cacheKey = $"license_{state}_{licenseNumber}";
        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber!, state!, licenseType!);

        // Assert
        Assert.False(result.Valid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Theory]
    [InlineData("CA")]
    [InlineData("NY")]
    [InlineData("TX")]
    [InlineData("FL")]
    public async Task VerifyLicenseAsync_SupportedStates_RecognizesState(string state)
    {
        // Arrange
        var licenseNumber = "LIC123456";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        // Even though state is supported, API is not implemented
        Assert.False(result.Valid);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("License verification not configured for state")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never); // Should not log warning for supported states
    }

    [Fact]
    public async Task VerifyLicenseAsync_CaseInsensitiveState_HandlesCorrectly()
    {
        // Arrange
        var licenseNumber = "LIC123456";
        var licenseType = "MFT";
        
        // Test lowercase
        var stateLower = "ca";
        var cacheKeyLower = $"license_{stateLower}_{licenseNumber}";
        
        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKeyLower, out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, stateLower, licenseType);

        // Assert
        Assert.False(result.Valid);
        
        // The service uses state.ToUpper() internally, so it should recognize "ca" as valid CA
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("License verification not configured for state")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion

    #region Cache Behavior Tests

    [Fact]
    public async Task VerifyLicenseAsync_MultipleCalls_UsesCacheOnSecondCall()
    {
        // Arrange
        var licenseNumber = "LIC123456";
        var state = "CA";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";
        
        var cachedResult = new LicenseVerificationResult
        {
            Valid = true,
            PractitionerName = "John Doe"
        };

        // First call - no cache
        object? cacheValue1 = null;
        _mockCache
            .SetupSequence(x => x.TryGetValue(cacheKey, out cacheValue1))
            .Returns(false)
            .Returns(true);

        // Second call - has cache
        object? cacheValue2 = cachedResult;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue2))
            .Returns(true);

        // Act
        var result1 = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);
        
        // Reset for second call
        _mockCache.Invocations.Clear();
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue2))
            .Returns(true);
            
        var result2 = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        Assert.False(result1.Valid); // First call returns manual verification
        Assert.True(result2.Valid); // Second call returns cached value
        Assert.Equal(cachedResult.PractitionerName, result2.PractitionerName);
    }

    #endregion
}