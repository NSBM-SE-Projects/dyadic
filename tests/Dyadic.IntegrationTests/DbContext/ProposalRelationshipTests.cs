using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Dyadic.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.IntegrationTests.DbContext;

public class ProposalRelationshipTests : IDisposable {
    private readonly Infrastructure.ApplicationDbContext _db;

    public ProposalRelationshipTests() {
        _db = TestDbContextFactory.Create();
    }

    [Fact]
    public async Task Proposal_BelongsToStudent_OneToOne() {
        var user = new ApplicationUser {
            Id = Guid.NewGuid(),
            FullName = "Student A",
            UserName = "a@test.com",
            Email = "a@test.com"
        };
        _db.Users.Add(user);

        var profile = new StudentProfile {
            UserId = user.Id,
            IndexNumber = "ST002", 
            Batch = "2026"
        };
        _db.StudentProfiles.Add(profile);

        var proposal = new Proposal {
            Title = "Rocket science in education",
            Description = "Exploring rockets and their physics",
            StudentId = profile.Id,
            Status = ProposalStatus.Draft
        };
        _db.Proposals.Add(proposal);
        await _db.SaveChangesAsync();

        var loaded = await _db.Proposals
            .Include(p => p.Student)
            .FirstAsync(p => p.Title == "Rocket science in education");

        loaded.Student.IndexNumber.Should().Be("ST002");
        loaded.Status.Should().Be(ProposalStatus.Draft);
    }

    [Fact]
    public async Task Supervisor_CanAcceptMultipleProposals() {
        var user = new ApplicationUser {
            Id = Guid.NewGuid(),
            FullName = "Dr. John",
            UserName = "john@test.com",
            Email = "john@test.com"
        };
        _db.Users.Add(user);

        var supervisor = new SupervisorProfile {
            UserId = user.Id,
            Department = "CS",
            MaxStudents = 3
        };
        _db.SupervisorProfiles.Add(supervisor);

        // Create 2 students with proposals assigned to this supervisor 
        for(int i = 1; i <= 2; i++) {
            var studentUser = new ApplicationUser {
                Id = Guid.NewGuid(),
                FullName = $"Student {i}",
                UserName = $"student{i}@test.com",
                Email = $"student{i}@test.com"
            };
            _db.Users.Add(studentUser);

            var studentProfile = new StudentProfile {
                UserId = studentUser.Id,
                IndexNumber = $"ST00{i}",
                Batch = "2026"
            };
            _db.StudentProfiles.Add(studentProfile);

            _db.Proposals.Add(new Proposal {
                Title = $"Proposal {i}",
                Description = $"Description {i}",
                StudentId = studentProfile.Id,
                SupervisorId = supervisor.Id,
                Status = ProposalStatus.Accepted
            });
        }

        await _db.SaveChangesAsync();

        var loaded = await _db.SupervisorProfiles
            .Include(sp => sp.AcceptedProposals)
            .FirstAsync(sp => sp.UserId == user.Id);

        loaded.AcceptedProposals.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(ProposalStatus.Draft)]
    [InlineData(ProposalStatus.Submitted)]
    [InlineData(ProposalStatus.Accepted)]
    [InlineData(ProposalStatus.Rejected)]
    [InlineData(ProposalStatus.Finalized)]
    public async Task ProposalStatus_RoundTrips_ThroughDatabase(ProposalStatus status) {
        var user = new ApplicationUser {
            Id = Guid.NewGuid(),
            FullName = "Round Trip Student",
            UserName = $"rt-{status}@test.com",
            Email = $"rt-{status}@test.com"
        };
        _db.Users.Add(user);

        var profile = new StudentProfile {
            UserId = user.Id,
            IndexNumber = "RT001",
            Batch = "2026"
        };
        _db.StudentProfiles.Add(profile);

        var proposal = new Proposal {
            Title = $"Test {status}",
            Description = "Round trip test",
            StudentId = profile.Id,
            Status = status
        };
        _db.Proposals.Add(proposal);
        await _db.SaveChangesAsync();

        _db.ChangeTracker.Clear();

        var loaded = await _db.Proposals.FindAsync(proposal.Id);
        loaded.Should().NotBeNull();
        loaded!.Status.Should().Be(status);
    }

    public void Dispose() => _db.Dispose();
}