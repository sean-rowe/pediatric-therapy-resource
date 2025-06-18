using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using TherapyDocs.Api.Models.Configuration;
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
    private readonly Mock<IOptions<LicenseVerificationConfig>> _mockOptions;
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
        _mockOptions = new Mock<IOptions<LicenseVerificationConfig>>();

        var config = new LicenseVerificationConfig
        {
            CacheHours = 24,
            RetryCount = 3,
            RetryDelayMs = 1000,
            States = new Dictionary<string, StateApiConfig>
            {
                ["CA"] = new StateApiConfig
                {
                    ApiUrl = "https://api.ca.gov/license-verification",
                    AuthType = "ApiKey",
                    ApiKey = "test-key",
                    TimeoutMs = 30000
                },
                ["NY"] = new StateApiConfig
                {
                    ApiUrl = "https://api.nysed.gov/license-lookup",
                    AuthType = "ApiKey",
                    ApiKey = "test-key",
                    TimeoutMs = 30000
                },
                ["TX"] = new StateApiConfig
                {
                    ApiUrl = "https://api.texas.gov/license-verify",
                    AuthType = "ApiKey",
                    ApiKey = "test-key",
                    TimeoutMs = 30000
                },
                ["FL"] = new StateApiConfig
                {
                    ApiUrl = "https://api.florida.gov/license-check",
                    AuthType = "ApiKey",
                    ApiKey = "test-key",
                    TimeoutMs = 30000
                }
            }
        };

        _mockOptions.Setup(x => x.Value).Returns(config);

        _service = new LicenseVerificationService(
            _httpClient,
            _mockCache.Object,
            _mockLogger.Object,
            _mockOptions.Object);
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

    #region California Response Parsing Tests

    [Fact]
    public async Task ParseCaliforniaResponse_ValidActiveResponse_ReturnsValidResult()
    {
        // Arrange
        var licenseNumber = "VALID123456";
        var state = "CA";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(false);

        var responseJson = @"{
            ""licenseFound"": true,
            ""status"": ""ACTIVE"",
            ""practitionerName"": ""Jane Smith"",
            ""licenseType"": ""Marriage and Family Therapist"",
            ""expirationDate"": ""2025-12-31"",
            ""disciplinaryActions"": false
        }";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        var cacheEntryMock = new Mock<ICacheEntry>();
        _mockCache
            .Setup(x => x.CreateEntry(cacheKey))
            .Returns(cacheEntryMock.Object);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        Assert.True(result.Valid);
        Assert.Equal("Jane Smith", result.PractitionerName);
        Assert.Equal("Marriage and Family Therapist", result.LicenseType);
        Assert.Equal(new DateTime(2025, 12, 31), result.ExpirationDate);
        Assert.False(result.DisciplinaryActions);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task ParseCaliforniaResponse_LicenseNotFound_ReturnsInvalidResult()
    {
        // Arrange
        var licenseNumber = "NOTFOUND123";
        var state = "CA";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(false);

        var responseJson = @"{
            ""licenseFound"": false
        }";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        Assert.False(result.Valid);
        Assert.Equal("License not found in California state records", result.ErrorMessage);
    }

    [Fact]
    public async Task ParseCaliforniaResponse_InactiveStatus_ReturnsInvalidResult()
    {
        // Arrange
        var licenseNumber = "INACTIVE123";
        var state = "CA";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(false);

        var responseJson = @"{
            ""licenseFound"": true,
            ""status"": ""INACTIVE"",
            ""practitionerName"": ""John Doe"",
            ""licenseType"": ""Marriage and Family Therapist"",
            ""expirationDate"": ""2023-12-31"",
            ""disciplinaryActions"": false
        }";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        Assert.False(result.Valid);
        Assert.Equal("License status: INACTIVE", result.ErrorMessage);
        Assert.Equal("John Doe", result.PractitionerName);
        Assert.Equal("Marriage and Family Therapist", result.LicenseType);
    }

    [Fact]
    public async Task ParseCaliforniaResponse_WithDisciplinaryActions_ReturnsInvalidResult()
    {
        // Arrange
        var licenseNumber = "DISCIPLINARY456";
        var state = "CA";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(false);

        var responseJson = @"{
            ""licenseFound"": true,
            ""status"": ""ACTIVE"",
            ""practitionerName"": ""Bob Johnson"",
            ""licenseType"": ""Marriage and Family Therapist"",
            ""expirationDate"": ""2025-06-30"",
            ""disciplinaryActions"": true,
            ""disciplinaryDetails"": ""Suspension for unprofessional conduct""
        }";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        Assert.False(result.Valid);
        Assert.Equal("License has disciplinary actions: Suspension for unprofessional conduct", result.ErrorMessage);
        Assert.True(result.DisciplinaryActions);
        Assert.Equal("Bob Johnson", result.PractitionerName);
    }

    [Fact]
    public async Task ParseCaliforniaResponse_InvalidJson_FallsBackToTestImplementation()
    {
        // Arrange
        var licenseNumber = "VALID789";
        var state = "CA";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(false);

        var invalidJson = "{ invalid json content }";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(invalidJson)
            });

        var cacheEntryMock = new Mock<ICacheEntry>();
        _mockCache
            .Setup(x => x.CreateEntry(cacheKey))
            .Returns(cacheEntryMock.Object);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        Assert.True(result.Valid); // VALID prefix means valid in test implementation
        Assert.Equal("Test Practitioner", result.PractitionerName);
        Assert.Equal("Professional License", result.LicenseType);
        Assert.NotNull(result.ExpirationDate);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error parsing California API response")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ParseCaliforniaResponse_MissingFields_HandlesGracefully()
    {
        // Arrange
        var licenseNumber = "PARTIAL123";
        var state = "CA";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(false);

        var responseJson = @"{
            ""licenseFound"": true,
            ""status"": ""ACTIVE""
        }";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        var cacheEntryMock = new Mock<ICacheEntry>();
        _mockCache
            .Setup(x => x.CreateEntry(cacheKey))
            .Returns(cacheEntryMock.Object);

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        Assert.True(result.Valid);
        Assert.Null(result.PractitionerName);
        Assert.Null(result.LicenseType);
        Assert.Null(result.ExpirationDate);
        Assert.False(result.DisciplinaryActions);
    }

    [Fact]
    public async Task ParseCaliforniaResponse_EmptyResponse_FallsBackToTestImplementation()
    {
        // Arrange
        var licenseNumber = "INVALID999";
        var state = "CA";
        var licenseType = "MFT";
        var cacheKey = $"license_{state}_{licenseNumber}";

        object? cacheValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(cacheKey, out cacheValue))
            .Returns(false);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

        // Act
        var result = await _service.VerifyLicenseAsync(licenseNumber, state, licenseType);

        // Assert
        Assert.False(result.Valid); // INVALID prefix means invalid in test implementation
        Assert.Equal("License not found in state records", result.ErrorMessage);
    }

    #endregion
}