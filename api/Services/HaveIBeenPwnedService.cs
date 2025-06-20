using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TherapyDocs.Api.Interfaces;

namespace TherapyDocs.Api.Services;

public class HaveIBeenPwnedService : IHaveIBeenPwnedService
{
    private const string ApiBaseUrl = "https://api.pwnedpasswords.com/range/";
    
    private readonly HttpClient _httpClient;
    private readonly ILogger<HaveIBeenPwnedService> _logger;

    public HaveIBeenPwnedService(HttpClient httpClient, ILogger<HaveIBeenPwnedService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(ApiBaseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TherapyDocs-API");
    }

    public async Task<bool> IsPasswordPwnedAsync(string password)
    {
        try
        {
            // Generate SHA1 hash of the password
            using var sha1 = SHA1.Create();
            var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();

            // Use k-anonymity model - send only first 5 characters
            var hashPrefix = hashString.Substring(0, 5);
            var hashSuffix = hashString.Substring(5);

            // Query the API
            var response = await _httpClient.GetAsync(hashPrefix);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HIBP API returned status {StatusCode}", response.StatusCode);
                // Fail open - if API is down, don't block registration
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var hashes = responseContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Check if our hash suffix appears in the response
            foreach (var line in hashes)
            {
                var parts = line.Split(':');
                if (parts.Length == 2 && parts[0].Equals(hashSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    var count = int.Parse(parts[1]);
                    if (count > 0)
                    {
                        _logger.LogInformation("Password found in breach database with {Count} occurrences", count);
                        return true;
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking password against HIBP database");
            // Fail open - don't block registration if service is unavailable
            return false;
        }
    }
}