using Dyadic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.Infrastructure;

public class ApplicationDbContext : DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<SupervisorProfile> SupervisorProfiles => Set<SupervisorProfile>();
    public DbSet<Proposal> Proposals => Set<Proposal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // User <> StudentProfile (1:1)
        modelBuilder.Entity<ApplicationUser>()
        .HasOne(u => u.StudentProfile)
        .WithOne(sp => sp.User)
        .HasForeignKey<StudentProfile>(sp => sp.UserId);

        // User <> SupervisorProfile (1:1)
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.SupervisorProfile)
            .WithOne(sp => sp.User)
            .HasForeignKey<SupervisorProfile>(sp => sp.UserId);

        // StudentProfile <> Proposal (1:1 - one proposal per student)
        modelBuilder.Entity<StudentProfile>()
            .HasOne(sp => sp.Proposal)
            .WithOne(p => p.Student)
            .HasForeignKey<Proposal>(p => p.StudentId);

        // SupervisorProfile <> Proposals (1:many - supervisor accepts multiple)
        modelBuilder.Entity<SupervisorProfile>()
            .HasMany(sp => sp.AcceptedProposals)
            .WithOne(p => p.Supervisor)
            .HasForeignKey(p => p.SupervisorId);

        // Unique Email
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Proposal>()
            .Property(p => p.Status)
            .HasConversion<string>();
    }
}