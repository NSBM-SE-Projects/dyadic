using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Dyadic.Infrastructure;
using Dyadic.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.UnitTests.Services;

public class ProposalServiceTests
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

    private static (ApplicationUser user, StudentProfile student, Proposal proposal)
        SeedProposal(ApplicationDbContext ctx, ProposalStatus status, Guid? supervisorId = null)
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Test Student", UserName = "student@test.com", Email = "student@test.com" };
        var student = new StudentProfile { Id = Guid.NewGuid(), UserId = user.Id, IndexNumber = "S001", Batch = "2024", User = user };
        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            Title = "Test Proposal",
            Abstract = "Test abstract",
            StudentId = student.Id,
            Status = status,
            SupervisorId = supervisorId,
            CreatedAt = DateTime.UtcNow
        };
        ctx.Users.Add(user);
        ctx.StudentProfiles.Add(student);
        ctx.Proposals.Add(proposal);
        ctx.SaveChanges();
        return (user, student, proposal);
    }

    private static (SupervisorProfile supervisor, ApplicationUser supUser)
        SeedSupervisor(ApplicationDbContext ctx, int maxStudents = 5, int existingAccepted = 0)
    {
        var supUser = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Test Supervisor", UserName = "sup@test.com", Email = "sup@test.com" };
        var supervisor = new SupervisorProfile { Id = Guid.NewGuid(), UserId = supUser.Id, Department = "CS", MaxStudents = maxStudents, User = supUser };
        ctx.Users.Add(supUser);
        ctx.SupervisorProfiles.Add(supervisor);

        for (int i = 0; i < existingAccepted; i++)
        {
            var p = new Proposal
            {
                Id = Guid.NewGuid(),
                Title = $"Existing {i}",
                Abstract = "x",
                StudentId = Guid.NewGuid(),
                SupervisorId = supervisor.Id,
                Status = ProposalStatus.Accepted
            };
            ctx.Proposals.Add(p);
        }

        ctx.SaveChanges();
        return (supervisor, supUser);
    }

    // ── positive tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitAsync_DraftProposal_BecomesSubmitted()
    {
        using var ctx = CreateContext();
        var (_, _, proposal) = SeedProposal(ctx, ProposalStatus.Draft);
        var svc = new ProposalService(ctx);

        var result = await svc.SubmitAsync(proposal.Id);

        result.Status.Should().Be(ProposalStatus.Submitted);
    }

    [Fact]
    public async Task WithdrawAsync_SubmittedProposal_BecomesDraft_AndWritesAuditEvent()
    {
        using var ctx = CreateContext();
        var (user, student, proposal) = SeedProposal(ctx, ProposalStatus.Submitted);
        var svc = new ProposalService(ctx);

        await svc.WithdrawAsync(proposal.Id, student.Id, user.Id);

        var saved = await ctx.Proposals.FindAsync(proposal.Id);
        saved!.Status.Should().Be(ProposalStatus.Draft);

        var evt = ctx.ProposalEvents.Single();
        evt.EventType.Should().Be(ProposalEventType.Withdrawn);
    }

    [Fact]
    public async Task AcceptProposalAsync_SubmittedProposal_BecomesAccepted_AndSetsSupervisorId()
    {
        using var ctx = CreateContext();
        var (_, _, proposal) = SeedProposal(ctx, ProposalStatus.Submitted);
        var (supervisor, _) = SeedSupervisor(ctx);
        var svc = new ProposalService(ctx);

        var result = await svc.AcceptProposalAsync(proposal.Id, supervisor.Id);

        result.Status.Should().Be(ProposalStatus.Accepted);
        result.SupervisorId.Should().Be(supervisor.Id);
    }

    [Fact]
    public async Task ConfirmMatchAsync_AcceptedProposal_BecomesFinalized_AndWritesAuditEvent()
    {
        using var ctx = CreateContext();
        var (supervisor, _) = SeedSupervisor(ctx);
        var (user, student, proposal) = SeedProposal(ctx, ProposalStatus.Accepted, supervisor.Id);
        var svc = new ProposalService(ctx);

        await svc.ConfirmMatchAsync(proposal.Id, student.Id, user.Id);

        var saved = await ctx.Proposals.FindAsync(proposal.Id);
        saved!.Status.Should().Be(ProposalStatus.Finalized);

        var evt = ctx.ProposalEvents.Single();
        evt.EventType.Should().Be(ProposalEventType.MatchConfirmed);
    }

    [Fact]
    public async Task RejectMatchAsync_AcceptedProposal_BecomesSubmitted_ClearsSupervisorId_AndWritesAuditEvent()
    {
        using var ctx = CreateContext();
        var (supervisor, _) = SeedSupervisor(ctx);
        var (user, student, proposal) = SeedProposal(ctx, ProposalStatus.Accepted, supervisor.Id);
        var svc = new ProposalService(ctx);

        await svc.RejectMatchAsync(proposal.Id, student.Id, user.Id);

        var saved = await ctx.Proposals.FindAsync(proposal.Id);
        saved!.Status.Should().Be(ProposalStatus.Submitted);
        saved.SupervisorId.Should().BeNull();

        var evt = ctx.ProposalEvents.Single();
        evt.EventType.Should().Be(ProposalEventType.MatchRejected);
    }

    // ── tamper-proof actorUserId ────────────────────────────────────────────────

    [Fact]
    public async Task WithdrawAsync_PersistsActorUserIdExactlyAsProvided()
    {
        using var ctx = CreateContext();
        var (user, student, proposal) = SeedProposal(ctx, ProposalStatus.Submitted);
        var svc = new ProposalService(ctx);
        var expectedActorId = user.Id;

        await svc.WithdrawAsync(proposal.Id, student.Id, expectedActorId);

        ctx.ProposalEvents.Single().ActorUserId.Should().Be(expectedActorId);
    }

    // ── negative tests ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(ProposalStatus.Draft)]
    [InlineData(ProposalStatus.Accepted)]
    [InlineData(ProposalStatus.Finalized)]
    public async Task WithdrawAsync_InvalidStatus_Throws(ProposalStatus status)
    {
        using var ctx = CreateContext();
        var (user, student, proposal) = SeedProposal(ctx, status);
        var svc = new ProposalService(ctx);

        var act = () => svc.WithdrawAsync(proposal.Id, student.Id, user.Id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData(ProposalStatus.Draft)]
    [InlineData(ProposalStatus.Accepted)]
    [InlineData(ProposalStatus.Finalized)]
    public async Task AcceptProposalAsync_InvalidStatus_Throws(ProposalStatus status)
    {
        using var ctx = CreateContext();
        var (_, _, proposal) = SeedProposal(ctx, status);
        var (supervisor, _) = SeedSupervisor(ctx);
        var svc = new ProposalService(ctx);

        var act = () => svc.AcceptProposalAsync(proposal.Id, supervisor.Id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData(ProposalStatus.Draft)]
    [InlineData(ProposalStatus.Submitted)]
    [InlineData(ProposalStatus.Finalized)]
    public async Task ConfirmMatchAsync_InvalidStatus_Throws(ProposalStatus status)
    {
        using var ctx = CreateContext();
        var (user, student, proposal) = SeedProposal(ctx, status);
        var svc = new ProposalService(ctx);

        var act = () => svc.ConfirmMatchAsync(proposal.Id, student.Id, user.Id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData(ProposalStatus.Draft)]
    [InlineData(ProposalStatus.Submitted)]
    [InlineData(ProposalStatus.Finalized)]
    public async Task RejectMatchAsync_InvalidStatus_Throws(ProposalStatus status)
    {
        using var ctx = CreateContext();
        var (user, student, proposal) = SeedProposal(ctx, status);
        var svc = new ProposalService(ctx);

        var act = () => svc.RejectMatchAsync(proposal.Id, student.Id, user.Id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AcceptProposalAsync_SupervisorAtCapacity_Throws()
    {
        using var ctx = CreateContext();
        var (_, _, proposal) = SeedProposal(ctx, ProposalStatus.Submitted);
        var (supervisor, _) = SeedSupervisor(ctx, maxStudents: 2, existingAccepted: 2);
        var svc = new ProposalService(ctx);

        var act = () => svc.AcceptProposalAsync(proposal.Id, supervisor.Id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*maximum student capacity*");
    }
}
