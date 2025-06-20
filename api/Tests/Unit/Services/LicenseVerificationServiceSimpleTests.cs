using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TherapyDocs.Api.Models.Configuration;
using TherapyDocs.Api.Services;
using Xunit;
using FluentAssertions;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class LicenseVerificationServiceSimpleTests
{
    private readonly LicenseVerificationService _service;
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ILogger<LicenseVerificationService>> _mockLogger;
    private readonly Mock<IOptions<LicenseVerificationConfig>> _mockConfig;

    public LicenseVerificationServiceSimpleTests()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<LicenseVerificationService>>();
        _mockConfig = new Mock<IOptions<LicenseVerificationConfig>>();
        
        var config = new LicenseVerificationConfig
        {
            CacheHours = 24,
            RetryCount = 3,
            RetryDelayMs = 1000,
            States = new Dictionary<string, StateApiConfig>()
        };
        _mockConfig.Setup(x => x.Value).Returns(config);
        
        // Use a real HttpClient since mocking it is complex
        var httpClient = new HttpClient();
        _service = new LicenseVerificationService(httpClient, _mockCache.Object, _mockLogger.Object, _mockConfig.Object);
    }

    [Fact]
    public async Task VerifyLicenseAsync_UnsupportedState_ReturnsFallbackResult()
    {
        // Arrange
        var licenseNumber = "12345";
        var state = "UNSUPPORTED";
        var licenseType = "speech_therapy";

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
    }

    [Theory]
    [InlineData("CA")]
    [InlineData("NY")]
    [InlineData("TX")]
    [InlineData("FL")]
    public async Task VerifyLicenseAsync_SupportedStatesWithoutApiCredentials_ReturnsFallbackResult(string state)
    {
        // Arrange
        var licenseNumber = "12345";
        var licenseType = "speech_therapy";

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
    }

    [Fact]
    public async Task VerifyLicenseAsync_ExceptionThrown_ReturnsFailureResult()
    {
        // Arrange  
        var licenseNumber = "12345";
        var state = "XX"; // Invalid state to trigger exception path
        var licenseType = "speech_therapy";

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
    }
}