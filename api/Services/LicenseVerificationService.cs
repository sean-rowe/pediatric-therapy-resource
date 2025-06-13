using Microsoft.Extensions.Caching.Memory;
using TherapyDocs.Api.Models.DTOs;
using System.Text.Json;

namespace TherapyDocs.Api.Services;

public class LicenseVerificationService : ILicenseVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LicenseVerificationService> _logger;
    private readonly IConfiguration _configuration;

    public LicenseVerificationService(
        HttpClient httpClient, 
        IMemoryCache cache, 
        ILogger<LicenseVerificationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<LicenseVerificationResult> VerifyLicenseAsync(string licenseNumber, string state, string licenseType)
    {
        var cacheKey = $"license_{state}_{licenseNumber}";
        
        // Check cache first
        if (_cache.TryGetValue(cacheKey, out LicenseVerificationResult? cachedResult))
        {
            _logger.LogInformation("License verification found in cache: {LicenseNumber}", licenseNumber);
            return cachedResult!;
        }

        try
        {
            var result = await VerifyLicenseWithStateApiAsync(licenseNumber, state, licenseType);
            
            // Cache successful verifications for 24 hours
            if (result.Valid)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromHours(24));
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying license: {LicenseNumber}, {State}", licenseNumber, state);
            
            // Return a fallback result that requires manual verification
            return new LicenseVerificationResult
            {
                Valid = false,
                ErrorMessage = "License verification service temporarily unavailable. Manual verification required."
            };
        }
    }

    private async Task<LicenseVerificationResult> VerifyLicenseWithStateApiAsync(string licenseNumber, string state, string licenseType)
    {
        // This is a mock implementation - in real scenarios, you'd integrate with actual state APIs
        // Each state has different APIs and formats for license verification
        
        var apiUrl = GetStateApiUrl(state);
        if (string.IsNullOrEmpty(apiUrl))
        {
            return new LicenseVerificationResult
            {
                Valid = false,
                ErrorMessage = $"License verification not available for state: {state}"
            };
        }

        // For demonstration, we'll simulate different responses
        // In real implementation, this would call the actual state API
        
        await Task.Delay(500); // Simulate API call delay
        
        // Mock verification logic
        var isValidFormat = licenseNumber.Length >= 4 && licenseNumber.All(c => char.IsLetterOrDigit(c) || c == '-');
        
        if (!isValidFormat)
        {
            return new LicenseVerificationResult
            {
                Valid = false,
                ErrorMessage = "Invalid license number format"
            };
        }

        // Simulate some licenses being invalid
        var hash = licenseNumber.GetHashCode();
        var isValid = Math.Abs(hash % 10) != 0; // 90% success rate for demo
        
        if (!isValid)
        {
            return new LicenseVerificationResult
            {
                Valid = false,
                ErrorMessage = "License not found in state database"
            };
        }

        return new LicenseVerificationResult
        {
            Valid = true,
            PractitionerName = "John Doe", // In real scenario, this would come from API
            LicenseType = licenseType,
            ExpirationDate = DateTime.Now.AddYears(2),
            DisciplinaryActions = false
        };
    }

    private string? GetStateApiUrl(string state)
    {
        // In real implementation, this would return actual state API URLs
        var stateApis = new Dictionary<string, string>
        {
            ["CA"] = "https://api.ca.gov/license-verification",
            ["NY"] = "https://api.nysed.gov/license-lookup",
            ["TX"] = "https://api.texas.gov/license-verify",
            ["FL"] = "https://api.florida.gov/license-check"
            // Add more states as needed
        };

        return stateApis.TryGetValue(state.ToUpper(), out var url) ? url : null;
    }
}