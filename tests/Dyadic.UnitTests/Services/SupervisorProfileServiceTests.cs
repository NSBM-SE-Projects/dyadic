using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Dyadic.Infrastructure;
using Dyadic.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.UnitTests.Services;

public class SupervisorProfileServiceTests
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

    private static ApplicationUser SeedUser(ApplicationDbContext ctx)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = "Test Supervisor",
            UserName = "sup@test.com",
            Email = "sup@test.com"
        };
        ctx.Users.Add(user);
        ctx.SaveChanges();
        return user;
    }

    private static SupervisorProfile SeedProfileWithAccepted(ApplicationDbContext ctx, Guid userId, int acceptedCount)
    {
        var profile = new SupervisorProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Department = "CS",
            MaxStudents = acceptedCount + 2,
            ResearchAreas = string.Empty
        };
        ctx.SupervisorProfiles.Add(profile);

        for (int i = 0; i < acceptedCount; i++)
        {
            var studentUser = new ApplicationUser { Id = Guid.NewGuid(), FullName = $"S{i}", UserName = $"s{i}@test.com", Email = $"s{i}@test.com" };
            var student = new StudentProfile { Id = Guid.NewGuid(), UserId = studentUser.Id, IndexNumber = $"S00{i}", Batch = "2024", User = studentUser };
            ctx.Users.Add(studentUser);
            ctx.StudentProfiles.Add(student);
            ctx.Proposals.Add(new Proposal
            {
                Id = Guid.NewGuid(), Title = $"P{i}", Abstract = "x",
                StudentId = student.Id, SupervisorId = profile.Id,
                Status = ProposalStatus.Accepted
            });
        }

        ctx.SaveChanges();
        return profile;
    }

    // ── positive tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrCreateProfileAsync_CreatesNewProfile_WithDefaults_WhenNoneExists()
    {
        using var ctx = CreateContext();
        var user = SeedUser(ctx);
        var svc = new SupervisorProfileService(ctx);

        var result = await svc.GetOrCreateProfileAsync(user.Id);

        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Department.Should().Be("N/A");
        result.MaxStudents.Should().Be(3);
        ctx.SupervisorProfiles.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOrCreateProfileAsync_ReturnsExistingProfile_WithoutCreatingDuplicate()
    {
        using var ctx = CreateContext();
        var user = SeedUser(ctx);
        var svc = new SupervisorProfileService(ctx);

        var first = await svc.GetOrCreateProfileAsync(user.Id);
        var second = await svc.GetOrCreateProfileAsync(user.Id);

        second.Id.Should().Be(first.Id);
        ctx.SupervisorProfiles.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateProfileAsync_PersistsAllFields()
    {
        using var ctx = CreateContext();
        var user = SeedUser(ctx);
        var svc = new SupervisorProfileService(ctx);
        await svc.GetOrCreateProfileAsync(user.Id); // ensure profile exists

        var result = await svc.UpdateProfileAsync(user.Id, "Engineering", "AI, ML", 5);

        result.Department.Should().Be("Engineering");
        result.ResearchAreas.Should().Be("AI, ML");
        result.MaxStudents.Should().Be(5);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithMaxStudentsEqualToAcceptedCount_IsAllowed()
    {
        using var ctx = CreateContext();
        var user = SeedUser(ctx);
        SeedProfileWithAccepted(ctx, user.Id, acceptedCount: 3);
        var svc = new SupervisorProfileService(ctx);

        // Setting MaxStudents == acceptedCount (3) should NOT throw
        var act = () => svc.UpdateProfileAsync(user.Id, "CS", "AI", 3);

        await act.Should().NotThrowAsync();
    }

    // ── negative tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfileAsync_WithMaxStudentsBelowAcceptedCount_Throws()
    {
        using var ctx = CreateContext();
        var user = SeedUser(ctx);
        SeedProfileWithAccepted(ctx, user.Id, acceptedCount: 3);
        var svc = new SupervisorProfileService(ctx);

        var act = () => svc.UpdateProfileAsync(user.Id, "CS", "AI", 2);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already accepted*");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithMaxStudentsZero_WhenAcceptedCountIsOne_Throws()
    {
        using var ctx = CreateContext();
        var user = SeedUser(ctx);
        SeedProfileWithAccepted(ctx, user.Id, acceptedCount: 1);
        var svc = new SupervisorProfileService(ctx);

        var act = () => svc.UpdateProfileAsync(user.Id, "CS", "AI", 0);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_UnknownUserId_AutoCreatesProfile_DoesNotThrow()
    {
        // UpdateProfileAsync calls GetOrCreateProfileAsync internally, which auto-creates
        // a profile when none exists — it never throws "not found".
        // This documents the actual behaviour: unknown userId creates a new profile.
        using var ctx = CreateContext();
        var svc = new SupervisorProfileService(ctx);
        var unknownUserId = Guid.NewGuid();

        var result = await svc.UpdateProfileAsync(unknownUserId, "CS", "AI", 3);

        result.Should().NotBeNull();
        result.UserId.Should().Be(unknownUserId);
        ctx.SupervisorProfiles.Should().HaveCount(1);
    }
}
