using Dyadic.Domain.Enums;

namespace Dyadic.Domain.Entities;

public class User {
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public StudentProfile? StudentProfile { get; set; }
    public SupervisorProfile? SupervisorProfile { get; set; }
}