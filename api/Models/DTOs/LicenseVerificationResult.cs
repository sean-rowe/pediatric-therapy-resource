namespace TherapyDocs.Api.Models.DTOs;

public class LicenseVerificationResult
{
    public bool Valid { get; set; }
    public string PractitionerName { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
    public bool DisciplinaryActions { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}