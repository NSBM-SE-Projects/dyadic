using Dyadic.Domain.Entities;
using Dyadic.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.IntegrationTests.DbContext;

public class UserProfileRelationshipTests : IDisposable {
    private readonly Infrastructure.ApplicationDbContext _db;

    public UserProfileRelationshipTests() {
        _db = TestDbContextFactory.Create();
    }

    [Fact]
    public async Task User_CanHaveStudentProfile_OneToOne() {
        var user = new ApplicationUser {
            Id = Guid.NewGuid(),
            FullName = "Test Student",
            UserName = "student@test.com",
            Email = "student@test.com"
        };
        _db.Users.Add(user);

        var profile = new StudentProfile {
            UserId = user.Id,
            IndexNumber = "ST001",
            Batch = "2026"
        };
        _db.StudentProfiles.Add(profile);
        await _db.SaveChangesAsync();

        var loaded = await _db.StudentProfiles
            .Include(sp => sp.User)
            .FirstAsync(sp => sp.UserId == user.Id);

        loaded.User.FullName.Should().Be("Test Student");
        loaded.IndexNumber.Should().Be("ST001");
    }

    [Fact]
    public async Task User_CanHaveSupervisorProfile_OneToOne() {
        var user = new ApplicationUser {
            Id = Guid.NewGuid(),
            FullName = "Test Supervisor",
            UserName = "supervisor@test.com",
            Email = "supervisor@test.com"
        };
        _db.Users.Add(user);

        var profile = new SupervisorProfile {
            UserId = user.Id,
            Department = "Computing",
            MaxStudents = 5
        };
        _db.SupervisorProfiles.Add(profile);
        await _db.SaveChangesAsync();

        var loaded = await _db.SupervisorProfiles
            .Include(sp => sp.User)
            .FirstAsync(sp => sp.UserId == user.Id);

        loaded.User.FullName.Should().Be("Test Supervisor");
        loaded.Department.Should().Be("Computing");
        loaded.MaxStudents.Should().Be(5);
    }

    public void Dispose() => _db.Dispose();
}