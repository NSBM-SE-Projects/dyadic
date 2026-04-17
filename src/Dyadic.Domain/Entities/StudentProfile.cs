namespace Dyadic.Domain.Entities;

public class StudentProfile {
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public required string IndexNumber { get; set; }
    public required string Batch { get; set; }

    public Proposal? Proposal { get; set; }
}