using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Dyadic.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid> {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<SupervisorProfile> SupervisorProfiles => Set<SupervisorProfile>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<AllocationOverride> AllocationOverrides => Set<AllocationOverride>();
    public DbSet<ResearchArea> ResearchAreas => Set<ResearchArea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

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

        // StudentProfile <> Proposal (1:1)
        modelBuilder.Entity<StudentProfile>()
            .HasOne(sp => sp.Proposal)
            .WithOne(p => p.Student)
            .HasForeignKey<Proposal>(p => p.StudentId);

        // SupervisorProfile <> Proposals (1:many)
        modelBuilder.Entity<SupervisorProfile>()
            .HasMany(sp => sp.AcceptedProposals)
            .WithOne(p => p.Supervisor)
            .HasForeignKey(p => p.SupervisorId);

        // ProposalStatus as string in DB
        modelBuilder.Entity<Proposal>()
            .Property(p => p.Status)
            .HasConversion<string>();

        // AllocationOverride FKs
        modelBuilder.Entity<AllocationOverride>()
            .HasOne(a => a.Proposal)
            .WithMany()
            .HasForeignKey(a => a.ProposalId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AllocationOverride>()
            .HasOne(a => a.PerformedBy)
            .WithMany()
            .HasForeignKey(a => a.PerformedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AllocationOverride>()
            .HasOne(a => a.OldSupervisor)
            .WithMany()
            .HasForeignKey(a => a.OldSupervisorId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AllocationOverride>()
            .HasOne(a => a.NewSupervisor)
            .WithMany()
            .HasForeignKey(a => a.NewSupervisorId)
            .OnDelete(DeleteBehavior.NoAction);

        // OverrideAction as string in DB
        modelBuilder.Entity<AllocationOverride>()
            .Property(a => a.Action)
            .HasConversion<string>();

        // ResearchArea unique name
        modelBuilder.Entity<ResearchArea>()
            .HasIndex(r => r.Name)
            .IsUnique();

        // ResearchArea <> Proposals (1:many)
        modelBuilder.Entity<ResearchArea>()
            .HasMany(r => r.Proposals)
            .WithOne(p => p.ResearchArea)
            .HasForeignKey(p => p.ResearchAreaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
