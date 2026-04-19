using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Dyadic.Infrastructure;
using Dyadic.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Dyadic.IntegrationTests.E2E;

[Trait("Category", "EndToEnd")]
public class AdminJourneyTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private Guid _proposalId;
    private Guid _newSupervisorProfileId;

    public AdminJourneyTests()
    {
        _factory = new TestWebApplicationFactory();
    }

    public async Task InitializeAsync()
    {
        // Seed two supervisors and one student + proposal assigned to supervisor A
        var supAUser = await _factory.SeedUserAsync("Dr. Alpha", "alpha@test.com", "Supervisor");
        var supBUser = await _factory.SeedUserAsync("Dr. Beta", "beta@test.com", "Supervisor");
        var studentUser = await _factory.SeedUserAsync("Charlie Student", "charlie@test.com", "Student");

        var supAProfileId = Guid.NewGuid();
        _newSupervisorProfileId = Guid.NewGuid();
        var studentProfileId = Guid.NewGuid();
        _proposalId = Guid.NewGuid();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.SupervisorProfiles.Add(new SupervisorProfile
        {
            Id = supAProfileId,
            UserId = supAUser.Id,
            Department = "CS",
            MaxStudents = 3
        });
        db.SupervisorProfiles.Add(new SupervisorProfile
        {
            Id = _newSupervisorProfileId,
            UserId = supBUser.Id,
            Department = "Math",
            MaxStudents = 3
        });
        db.StudentProfiles.Add(new StudentProfile
        {
            Id = studentProfileId,
            UserId = studentUser.Id,
            IndexNumber = "ST201",
            Batch = "2026"
        });
        db.Proposals.Add(new Proposal
        {
            Id = _proposalId,
            Title = "Admin Journey Test Proposal",
            Abstract = "This research covers advanced topics in cybersecurity and threat detection systems.",
            TechStack = "Python, Snort",
            StudentId = studentProfileId,
            SupervisorId = supAProfileId,
            Status = ProposalStatus.Accepted
        });

        await db.SaveChangesAsync();

        // Login as the seeded admin (admin@dyadic.local / Admin@123456)
        _client = await _factory.GetAuthenticatedClientAsync("admin@dyadic.local", "Admin@123456");
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Admin_CanViewDashboard_ReassignProposal_AndSeeAuditLog()
    {
        // Step 1 — GET /Admin/Dashboard and assert proposal is visible
        var dashGet = await _client.GetAsync("/Admin/Dashboard");
        dashGet.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var dashHtml = await dashGet.Content.ReadAsStringAsync();
        dashHtml.Should().Contain("Admin Journey Test Proposal");

        // Step 2 — POST Reassign
        var token = TestWebApplicationFactory.ExtractAntiforgeryToken(dashHtml);
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["ProposalId"]                 = _proposalId.ToString(),
            ["NewSupervisorId"]            = _newSupervisorProfileId.ToString(),
            ["Reason"]                     = "Reassigning for workload balancing purposes",
            ["__RequestVerificationToken"] = token
        });
        var reassignResponse = await _client.PostAsync("/Admin/Dashboard?handler=Reassign", form);

        reassignResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect);

        // Step 3 — GET /Admin/AuditLog and assert the Reassign event is listed
        var auditResponse = await _client.GetAsync("/Admin/AuditLog");
        auditResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var auditHtml = await auditResponse.Content.ReadAsStringAsync();
        auditHtml.Should().Contain("Reassign");
    }
}
