namespace Dyadic.Application.DTOs;

public class AuditLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string ActorRole { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ProposalTitle { get; set; } = string.Empty;
    public string? OldSupervisor { get; set; }
    public string? NewSupervisor { get; set; }
    public string? Reason { get; set; }
}
