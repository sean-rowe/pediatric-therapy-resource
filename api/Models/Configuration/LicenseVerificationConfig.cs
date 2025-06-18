namespace TherapyDocs.Api.Models.Configuration;

public class LicenseVerificationConfig
{
    public const string SectionName = "LicenseVerification";
    
    public Dictionary<string, StateApiConfig> States { get; set; } = new();
    public int CacheHours { get; set; } = 24;
    public int RetryCount { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
}

public class StateApiConfig
{
    public string ApiUrl { get; set; } = string.Empty;
    public string AuthType { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public int TimeoutMs { get; set; } = 30000;
}