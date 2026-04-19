using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Dyadic.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.IntegrationTests.SqlServer;

[Trait("Category", "SqlServer")]
public class SchemaEnforcementTests : IAsyncLifetime
{
    private readonly SqlServerDbContextFactory _factory = new();
    private Infrastructure.ApplicationDbContext _db = null!;

    public async Task InitializeAsync() => _db = await _factory.CreateAsync();
    public async Task DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task UniqueIndex_OnResearchAreaName_RejectsDuplicate()
    {
        _db.ResearchAreas.Add(new ResearchArea { Id = Guid.NewGuid(), Name = "Duplicate Area", IsActive = true, CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        _db.ResearchAreas.Add(new ResearchArea { Id = Guid.NewGuid(), Name = "Duplicate Area", IsActive = true, CreatedAt = DateTime.UtcNow });

        var act = () => _db.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task ProposalStatus_IsStoredAsString()
    {
        var user = SeedUser();
        var student = new StudentProfile { Id = Guid.NewGuid(), UserId = user.Id, IndexNumber = "S001", Batch = "2024", User = user };
        var proposal = new Proposal { Id = Guid.NewGuid(), Title = "Test", Abstract = "Test", StudentId = student.Id, Status = ProposalStatus.Draft };
        _db.Users.Add(user);
        _db.StudentProfiles.Add(student);
        _db.Proposals.Add(proposal);
        await _db.SaveChangesAsync();

        var raw = await _db.Database
            .SqlQueryRaw<string>("SELECT CAST([Status] AS nvarchar(50)) AS [Value] FROM [Proposals] WHERE [Id] = {0}", proposal.Id)
            .FirstAsync();

        raw.Should().Be("Draft");
    }

    [Fact]
    public async Task OverrideAction_IsStoredAsString()
    {
        var (adminUser, proposal, supervisor) = await SeedForOverride();

        var audit = new AllocationOverride
        {
            Id = Guid.NewGuid(),
            ProposalId = proposal.Id,
            PerformedByUserId = adminUser.Id,
            Action = OverrideAction.Reassign,
            OldSupervisorId = null,
            NewSupervisorId = supervisor.Id,
            Reason = "Test",
            Timestamp = DateTime.UtcNow
        };
        _db.AllocationOverrides.Add(audit);
        await _db.SaveChangesAsync();

        var raw = await _db.Database
            .SqlQueryRaw<string>("SELECT CAST([Action] AS nvarchar(50)) AS [Value] FROM [AllocationOverrides] WHERE [Id] = {0}", audit.Id)
            .FirstAsync();

        raw.Should().Be("Reassign");
    }

    [Fact]
    public async Task ProposalEventType_IsStoredAsString()
    {
        var user = SeedUser();
        var student = new StudentProfile { Id = Guid.NewGuid(), UserId = user.Id, IndexNumber = "S002", Batch = "2024", User = user };
        var proposal = new Proposal { Id = Guid.NewGuid(), Title = "Event Test", Abstract = "Test", StudentId = student.Id, Status = ProposalStatus.Submitted };
        _db.Users.Add(user);
        _db.StudentProfiles.Add(student);
        _db.Proposals.Add(proposal);
        await _db.SaveChangesAsync();

        var evt = new ProposalEvent
        {
            Id = Guid.NewGuid(),
            ProposalId = proposal.Id,
            ActorUserId = user.Id,
            EventType = ProposalEventType.Withdrawn,
            Timestamp = DateTime.UtcNow
        };
        _db.ProposalEvents.Add(evt);
        await _db.SaveChangesAsync();

        var raw = await _db.Database
            .SqlQueryRaw<string>("SELECT CAST([EventType] AS nvarchar(50)) AS [Value] FROM [ProposalEvents] WHERE [Id] = {0}", evt.Id)
            .FirstAsync();

        raw.Should().Be("Withdrawn");
    }

    // ── seed helpers ───────────────────────────────────────────────────────────

    private static ApplicationUser SeedUser(string suffix = "") =>
        new() { Id = Guid.NewGuid(), FullName = $"User{suffix}", UserName = $"u{suffix}@test.com", Email = $"u{suffix}@test.com" };

    private async Task<(ApplicationUser admin, Proposal proposal, SupervisorProfile supervisor)> SeedForOverride()
    {
        var adminUser = SeedUser("admin");
        var studentUser = SeedUser("student");
        var supUser = SeedUser("sup");
        var student = new StudentProfile { Id = Guid.NewGuid(), UserId = studentUser.Id, IndexNumber = "S003", Batch = "2024", User = studentUser };
        var supervisor = new SupervisorProfile { Id = Guid.NewGuid(), UserId = supUser.Id, Department = "CS", MaxStudents = 5, User = supUser };
        var proposal = new Proposal { Id = Guid.NewGuid(), Title = "Override Test", Abstract = "Test", StudentId = student.Id, Status = ProposalStatus.Accepted, SupervisorId = supervisor.Id };
        _db.Users.AddRange(adminUser, studentUser, supUser);
        _db.StudentProfiles.Add(student);
        _db.SupervisorProfiles.Add(supervisor);
        _db.Proposals.Add(proposal);
        await _db.SaveChangesAsync();
        return (adminUser, proposal, supervisor);
    }
}
