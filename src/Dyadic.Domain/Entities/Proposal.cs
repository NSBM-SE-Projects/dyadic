using Dyadic.Domain.Enums;

namespace Dyadic.Domain.Entities;

public class Proposal {
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }

    public Guid StudentId { get; set; }
    public StudentProfile Student { get; set; } = null!;

    public Guid? SupervisorId { get; set; }
    public SupervisorProfile? Supervisor { get; set; }

    public ProposalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

