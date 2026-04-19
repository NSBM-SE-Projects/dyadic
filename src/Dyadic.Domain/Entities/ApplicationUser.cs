using Microsoft.AspNetCore.Identity;

namespace Dyadic.Domain.Entities;

public class ApplicationUser: IdentityUser<Guid> {
    public required string FullName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public StudentProfile? StudentProfile { get; set; }
    public SupervisorProfile? SupervisorProfile { get; set; } 
}