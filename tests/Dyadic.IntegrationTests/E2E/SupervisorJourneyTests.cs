using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Dyadic.Infrastructure;
using Dyadic.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dyadic.IntegrationTests.E2E;

[Trait("Category", "EndToEnd")]
public class SupervisorJourneyTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private Guid _proposalId;

    public SupervisorJourneyTests()
    {
        _factory = new TestWebApplicationFactory();
    }

    public async Task InitializeAsync()
    {
        var supervisorUser = await _factory.SeedUserAsync("Dr. Smith", "smith@test.com", "Supervisor");

        // Seed a student profile + submitted proposal directly via DbContext.
        // FK constraints are not enforced by InMemory so we use a stub student user ID.
        var studentProfileId = Guid.NewGuid();
        _proposalId = Guid.NewGuid();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.StudentProfiles.Add(new StudentProfile
        {
            Id = studentProfileId,
            UserId = Guid.NewGuid(),
            IndexNumber = "ST001",
            Batch = "2026"
        });

        db.Proposals.Add(new Proposal
        {
            Id = _proposalId,
            Title = "Supervisor Journey Test Proposal",
            Abstract = "This research explores advanced topics in distributed systems and cloud computing architectures.",
            TechStack = "Go, Kubernetes",
            StudentId = studentProfileId,
            Status = ProposalStatus.Submitted
        });

        await db.SaveChangesAsync();

        _client = await _factory.GetAuthenticatedClientAsync("smith@test.com");
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Supervisor_AcceptsProposal_RedirectsToAcceptedProposals_AndStatusIsAccepted()
    {
        // GET dashboard to obtain antiforgery token
        var getResponse = await _client.GetAsync("/Supervisor/Dashboard");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var token = TestWebApplicationFactory.ExtractAntiforgeryToken(
            await getResponse.Content.ReadAsStringAsync());

        // POST to accept the proposal (default OnPostAsync handler)
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["proposalId"]                 = _proposalId.ToString(),
            ["__RequestVerificationToken"] = token
        });
        var response = await _client.PostAsync("/Supervisor/Dashboard", form);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/Supervisor/AcceptedProposals");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var proposal = await db.Proposals.FindAsync(_proposalId);
        proposal.Should().NotBeNull();
        proposal!.Status.Should().Be(ProposalStatus.Accepted);
        proposal.SupervisorId.Should().NotBeNull();
    }
}
