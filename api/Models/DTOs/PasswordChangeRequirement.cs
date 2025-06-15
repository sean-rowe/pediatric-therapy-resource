namespace TherapyDocs.Api.Models.DTOs;

public class PasswordChangeRequirement
{
    public bool ChangeRequired { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? LastPasswordChangeDate { get; set; }
    public int DaysSinceLastChange { get; set; }
}