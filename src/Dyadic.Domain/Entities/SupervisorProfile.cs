namespace Dyadic.Domain.Entities;

public class SupervisorProfile {
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public required string Department { get; set; }
    public int MaxStudents { get; set; }

    public ICollection<Proposal> AcceptedProposals { get; set; } = new List<Proposal>();
}