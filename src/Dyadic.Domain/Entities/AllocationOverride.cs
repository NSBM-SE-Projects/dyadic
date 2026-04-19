using Dyadic.Domain.Enums;

namespace Dyadic.Domain.Entities;

public class AllocationOverride
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public Proposal Proposal { get; set; } = null!;
    public Guid PerformedByUserId { get; set; }
    public ApplicationUser PerformedBy { get; set; } = null!;
    public OverrideAction Action { get; set; }
    public Guid? OldSupervisorId { get; set; }
    public Guid? NewSupervisorId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
