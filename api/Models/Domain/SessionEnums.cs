namespace UPTRMS.Api.Models.Domain;

public enum SessionStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}

public enum StudentStatus
{
    Active,
    Inactive,
    Discharged,
    OnHold
}

public enum AlertType
{
    NoRecentSession,
    GoalDeadline,
    MissedSession,
    DocumentationDue,
    LowProgress
}

public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}