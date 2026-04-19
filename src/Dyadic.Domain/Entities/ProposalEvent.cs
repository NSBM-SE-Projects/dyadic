using Dyadic.Domain.Enums;

namespace Dyadic.Domain.Entities;

public class ProposalEvent
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public Proposal Proposal { get; set; } = null!;
    public Guid ActorUserId { get; set; }
    public ApplicationUser Actor { get; set; } = null!;
    public ProposalEventType EventType { get; set; }
    public Guid? RelatedSupervisorId { get; set; }
    public SupervisorProfile? RelatedSupervisor { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
