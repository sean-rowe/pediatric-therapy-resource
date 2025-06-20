namespace TherapyDocs.Api.Models;

public class PasswordChangeRequirement
{
    public bool IsRequired { get; set; }
    
    public DateTime? LastPasswordChangeDate { get; set; }
    
    public int DaysSinceLastChange { get; set; }
    
    public string Reason { get; set; } = string.Empty;
}