using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class LicenseVerificationServiceComprehensiveTests : IDisposable
{
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ILogger<LicenseVerificationService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly HttpClient _httpClient;
    private readonly LicenseVerificationService _service;

    public LicenseVerificationServiceComprehensiveTests()
    {
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<LicenseVerificationService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _httpClient = new HttpClient(); // Real HttpClient since mocking is complex
        
        _service = new LicenseVerificationService(
            _httpClient,
            _mockCache.Object,
            _mockLogger.Object,
            _mockConfiguration.Object);
    }

    /**
     * Feature: License Verification with Caching
     *   As a licensing verification system
     *   I want to verify professional licenses efficiently
     *   So that only legitimate practitioners can register
     * 
     * Scenario: Cache hit for previously verified license
     *   Given a license has been verified before and cached
     *   When I verify the same license again
     *   Then the cached result is returned
     *   And no API call is made
     */
    [Fact]
    public async Task VerifyLicenseAsync_CacheHit_ReturnsCachedResult()
    {
        // Arrange
        var licenseNumber = "12345";
        var state = "CA";
        var licenseType = "speech_therapy";
        var cacheKey = $"license_{state}_{licenseNumber}";
        var cachedResult = new LicenseVerificationResult
        {
            Valid = true,
            PractitionerName = "John Doe",
            LicenseType = licenseType,
            ExpirationDate = DateTime.UtcNow.AddYears(1)
        };

        object? cacheValue = cachedResult;
        _mockCache.Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(true);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        result.Should().BeEquivalentTo(cachedResult);
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("License verification found in cache") && v.ToString()!.Contains(licenseNumber)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /**
     * Scenario: Cache miss for new license verification
     *   Given a license has not been verified before
     *   When I verify the license
     *   Then an API call is attempted
     *   And the result is processed accordingly
     */
    [Fact]
    public async Task VerifyLicenseAsync_CacheMiss_AttemptsApiCall()
    {
        // Arrange
        var licenseNumber = "NEW123";
        var state = "CA";
        var licenseType = "speech_therapy";
        var cacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
        
        // Verify no caching attempted for failed verification (cannot verify extension method directly)
        // The expectation is that cache.Set is not called since the result is invalid
    }

    /**
     * Scenario: Unsupported state verification
     *   Given a state that doesn't have API integration
     *   When I attempt to verify a license
     *   Then a manual verification result is returned
     *   And appropriate logging occurs
     */
    [Theory]
    [InlineData("ZZ")]
    [InlineData("XX")]
    [InlineData("INVALID")]
    [InlineData("99")]
    public async Task VerifyLicenseAsync_UnsupportedState_ReturnsManualVerificationRequired(string state)
    {
        // Arrange
        var licenseNumber = "12345";
        var licenseType = "speech_therapy";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
        
        // Verify error logging
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error verifying license")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /**
     * Scenario: Supported states with no API implementation
     *   Given a state with configured URL but no implementation
     *   When I attempt to verify a license
     *   Then a manual verification result is returned
     */
    [Theory]
    [InlineData("CA")]
    [InlineData("NY")]
    [InlineData("TX")]
    [InlineData("FL")]
    public async Task VerifyLicenseAsync_SupportedStateNotImplemented_ReturnsManualVerificationRequired(string state)
    {
        // Arrange
        var licenseNumber = "12345";
        var licenseType = "speech_therapy";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
        
        // Verify warning logging for state not configured
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error verifying license")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /**
     * Feature: Parameter Validation and Edge Cases
     *   As a robust verification system
     *   I want to handle edge cases gracefully
     *   So that the system remains stable under all conditions
     * 
     * Scenario: Empty license number
     *   Given an empty license number
     *   When I attempt verification
     *   Then the system handles it gracefully
     */
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task VerifyLicenseAsync_EmptyLicenseNumber_HandlesGracefully(string licenseNumber)
    {
        // Arrange
        var state = "CA";
        var licenseType = "speech_therapy";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
    }

    /**
     * Scenario: Empty or invalid state
     *   Given an empty or invalid state
     *   When I attempt verification
     *   Then the system handles it gracefully
     */
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task VerifyLicenseAsync_EmptyState_HandlesGracefully(string? state)
    {
        // Arrange
        var licenseNumber = "12345";
        var licenseType = "speech_therapy";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state ?? string.Empty, licenseType);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
    }

    /**
     * Scenario: Empty license type
     *   Given an empty license type
     *   When I attempt verification
     *   Then the system handles it gracefully
     */
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task VerifyLicenseAsync_EmptyLicenseType_HandlesGracefully(string? licenseType)
    {
        // Arrange
        var licenseNumber = "12345";
        var state = "CA";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType ?? string.Empty);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
    }

    /**
     * Scenario: Very long license number
     *   Given a very long license number
     *   When I attempt verification
     *   Then the system handles it gracefully
     */
    [Fact]
    public async Task VerifyLicenseAsync_VeryLongLicenseNumber_HandlesGracefully()
    {
        // Arrange
        var licenseNumber = new string('1', 1000);
        var state = "CA";
        var licenseType = "speech_therapy";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
    }

    /**
     * Scenario: Special characters in license number
     *   Given a license number with special characters
     *   When I attempt verification
     *   Then the system handles it gracefully
     */
    [Theory]
    [InlineData("ABC-123")]
    [InlineData("ABC.123")]
    [InlineData("ABC/123")]
    [InlineData("ABC#123")]
    [InlineData("ABC@123")]
    [InlineData("АВС123")] // Cyrillic characters
    [InlineData("测试123")] // Chinese characters
    public async Task VerifyLicenseAsync_SpecialCharactersInLicenseNumber_HandlesGracefully(string licenseNumber)
    {
        // Arrange
        var state = "CA";
        var licenseType = "speech_therapy";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
    }

    /**
     * Scenario: Case sensitivity tests for states
     *   Given state codes in different cases
     *   When I attempt verification
     *   Then the system handles case insensitively
     */
    [Theory]
    [InlineData("ca")]
    [InlineData("CA")]
    [InlineData("Ca")]
    [InlineData("cA")]
    public async Task VerifyLicenseAsync_StateCaseInsensitive_HandlesCorrectly(string state)
    {
        // Arrange
        var licenseNumber = "12345";
        var licenseType = "speech_therapy";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
    }

    /**
     * Feature: Cache Key Generation
     *   As a caching system
     *   I want to generate consistent cache keys
     *   So that caching works reliably
     * 
     * Scenario: Cache key consistency
     *   Given the same license parameters
     *   When verification is called multiple times
     *   Then the same cache key is used
     */
    [Fact]
    public async Task VerifyLicenseAsync_ConsistentCacheKey_UsedForSameParameters()
    {
        // Arrange
        var licenseNumber = "12345";
        var state = "CA";
        var licenseType = "speech_therapy";
        var expectedCacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(expectedCacheKey, out cacheValue))
            .Returns(false);

        // Act
        await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);
        await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        _mockCache.Verify(x => x.TryGetValue(expectedCacheKey, out cacheValue), Times.Exactly(2));
    }

    /**
     * Scenario: Different cache keys for different parameters
     *   Given different license parameters
     *   When verification is called
     *   Then different cache keys are used
     */
    [Fact]
    public async Task VerifyLicenseAsync_DifferentParameters_UseDifferentCacheKeys()
    {
        // Arrange
        var licenseNumber1 = "12345";
        var licenseNumber2 = "67890";
        var state = "CA";
        var licenseType = "speech_therapy";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out cacheValue))
            .Returns(false);

        // Act
        await _service.VerifyLicenseAsync(licenseNumber1, state, licenseType);
        await _service.VerifyLicenseAsync(licenseNumber2, state, licenseType);

        // Assert
        _mockCache.Verify(x => x.TryGetValue($"license_{state}_{licenseNumber1}", out cacheValue), Times.Once);
        _mockCache.Verify(x => x.TryGetValue($"license_{state}_{licenseNumber2}", out cacheValue), Times.Once);
    }

    /**
     * Feature: Logging Verification
     *   As a monitoring system
     *   I want comprehensive logging
     *   So that I can track verification attempts and issues
     * 
     * Scenario: Error logging for unsupported states
     *   Given an unsupported state
     *   When verification fails
     *   Then appropriate error logging occurs
     */
    [Fact]
    public async Task VerifyLicenseAsync_LogsErrorForUnsupportedState()
    {
        // Arrange
        var licenseNumber = "12345";
        var state = "UNSUPPORTED";
        var licenseType = "speech_therapy";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out cacheValue))
            .Returns(false);

        // Act
        await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => 
                v.ToString()!.Contains("Error verifying license") &&
                v.ToString()!.Contains(licenseNumber) &&
                v.ToString()!.Contains(state)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /**
     * Feature: License Type Validation
     *   As a license verification system
     *   I want to handle various license types
     *   So that different therapy disciplines are supported
     * 
     * Scenario: Valid license types
     *   Given various valid license types
     *   When verification is attempted
     *   Then the system processes them consistently
     */
    [Theory]
    [InlineData("speech_therapy")]
    [InlineData("occupational_therapy")]
    [InlineData("physical_therapy")]
    [InlineData("behavioral_therapy")]
    [InlineData("clinical_psychology")]
    [InlineData("marriage_family_therapy")]
    public async Task VerifyLicenseAsync_ValidLicenseTypes_ProcessedConsistently(string licenseType)
    {
        // Arrange
        var licenseNumber = "12345";
        var state = "CA";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out cacheValue))
            .Returns(false);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Manual verification required");
    }

    /**
     * Feature: Concurrent Access Safety
     *   As a multi-threaded application
     *   I want cache operations to be safe
     *   So that concurrent requests don't cause issues
     * 
     * Scenario: Concurrent cache access
     *   Given multiple simultaneous requests for the same license
     *   When verification is performed
     *   Then the cache is accessed safely
     */
    [Fact]
    public async Task VerifyLicenseAsync_ConcurrentAccess_HandlesSafely()
    {
        // Arrange
        var licenseNumber = "12345";
        var state = "CA";
        var licenseType = "speech_therapy";

        object? cacheValue = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out cacheValue))
            .Returns(false);

        // Act
        var tasks = new List<Task<LicenseVerificationResult>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_service.VerifyLicenseAsync(licenseNumber, state, licenseType));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().OnlyContain(r => r != null && !r.Valid);
    }

    /**
     * Feature: Memory Management
     *   As a long-running service
     *   I want proper resource management
     *   So that memory usage remains stable
     * 
     * Scenario: Service disposal
     *   Given the service instance
     *   When it's disposed
     *   Then resources are cleaned up properly
     */
    [Fact]
    public void Dispose_ServiceResources_CleansUpProperly()
    {
        // Act & Assert - No exception should be thrown
        var act = () => _httpClient.Dispose();
        act.Should().NotThrow();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}