using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Dyadic.Infrastructure;
using Dyadic.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.UnitTests.Services;

public class AdminServiceTests
{
    // ── helpers ────────────────────────────────────────────────────────────────

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static Proposal SeedProposal(ApplicationDbContext ctx, ProposalStatus status, Guid? supervisorId = null)
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Student", UserName = "s@test.com", Email = "s@test.com" };
        var student = new StudentProfile { Id = Guid.NewGuid(), UserId = user.Id, IndexNumber = "S001", Batch = "2024", User = user };
        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Abstract = "Abstract",
            StudentId = student.Id,
            Status = status,
            SupervisorId = supervisorId,
            CreatedAt = DateTime.UtcNow
        };
        ctx.Users.Add(user);
        ctx.StudentProfiles.Add(student);
        ctx.Proposals.Add(proposal);
        ctx.SaveChanges();
        return proposal;
    }

    private static SupervisorProfile SeedSupervisor(ApplicationDbContext ctx, int maxStudents = 5, int existingAccepted = 0)
    {
        var supUser = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Supervisor", UserName = "sup@test.com", Email = "sup@test.com" };
        var supervisor = new SupervisorProfile { Id = Guid.NewGuid(), UserId = supUser.Id, Department = "CS", MaxStudents = maxStudents, User = supUser };
        ctx.Users.Add(supUser);
        ctx.SupervisorProfiles.Add(supervisor);

        for (int i = 0; i < existingAccepted; i++)
        {
            ctx.Proposals.Add(new Proposal
            {
                Id = Guid.NewGuid(), Title = $"P{i}", Abstract = "x",
                StudentId = Guid.NewGuid(), SupervisorId = supervisor.Id,
                Status = ProposalStatus.Accepted
            });
        }

        ctx.SaveChanges();
        return supervisor;
    }

    private static Guid SeedAdminUser(ApplicationDbContext ctx)
    {
        var admin = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Admin", UserName = "admin@test.com", Email = "admin@test.com" };
        ctx.Users.Add(admin);
        ctx.SaveChanges();
        return admin.Id;
    }

    // ── positive tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ReassignProposalAsync_UpdatesSupervisorId_AndWritesAllocationOverride()
    {
        using var ctx = CreateContext();
        var oldSup = SeedSupervisor(ctx);
        var newSup = SeedSupervisor(ctx);
        var proposal = SeedProposal(ctx, ProposalStatus.Accepted, oldSup.Id);
        var adminId = SeedAdminUser(ctx);
        var svc = new AdminService(ctx);

        await svc.ReassignProposalAsync(proposal.Id, newSup.Id, adminId, "Workload balancing");

        var saved = await ctx.Proposals.FindAsync(proposal.Id);
        saved!.SupervisorId.Should().Be(newSup.Id);
        saved.Status.Should().Be(ProposalStatus.Accepted);

        var audit = ctx.AllocationOverrides.Single();
        audit.Action.Should().Be(OverrideAction.Reassign);
        audit.OldSupervisorId.Should().Be(oldSup.Id);
        audit.NewSupervisorId.Should().Be(newSup.Id);
    }

    [Fact]
    public async Task UnmatchProposalAsync_ClearsSupervisorId_SetsSubmitted_AndWritesAllocationOverride()
    {
        using var ctx = CreateContext();
        var supervisor = SeedSupervisor(ctx);
        var proposal = SeedProposal(ctx, ProposalStatus.Finalized, supervisor.Id);
        var adminId = SeedAdminUser(ctx);
        var svc = new AdminService(ctx);

        await svc.UnmatchProposalAsync(proposal.Id, adminId, "Student request");

        var saved = await ctx.Proposals.FindAsync(proposal.Id);
        saved!.SupervisorId.Should().BeNull();
        saved.Status.Should().Be(ProposalStatus.Submitted);

        var audit = ctx.AllocationOverrides.Single();
        audit.Action.Should().Be(OverrideAction.Unmatch);
        audit.OldSupervisorId.Should().Be(supervisor.Id);
        audit.NewSupervisorId.Should().BeNull();
    }

    // ── negative tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ReassignProposalAsync_NewSupervisorAtCapacity_Throws()
    {
        using var ctx = CreateContext();
        var oldSup = SeedSupervisor(ctx);
        var fullSup = SeedSupervisor(ctx, maxStudents: 2, existingAccepted: 2);
        var proposal = SeedProposal(ctx, ProposalStatus.Accepted, oldSup.Id);
        var adminId = SeedAdminUser(ctx);
        var svc = new AdminService(ctx);

        var act = () => svc.ReassignProposalAsync(proposal.Id, fullSup.Id, adminId, "Test");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*maximum capacity*");
    }

    [Theory]
    [InlineData(ProposalStatus.Draft)]
    [InlineData(ProposalStatus.Submitted)]
    public async Task UnmatchProposalAsync_NoSupervisorAssigned_StillRunsWithoutError(ProposalStatus status)
    {
        // UnmatchAsync doesn't guard on status — it clears whatever supervisor is set.
        // For Draft/Submitted the supervisor is already null; the override row still writes.
        using var ctx = CreateContext();
        var proposal = SeedProposal(ctx, status, supervisorId: null);
        var adminId = SeedAdminUser(ctx);
        var svc = new AdminService(ctx);

        await svc.UnmatchProposalAsync(proposal.Id, adminId, "Test");

        ctx.AllocationOverrides.Should().HaveCount(1);
    }
}
