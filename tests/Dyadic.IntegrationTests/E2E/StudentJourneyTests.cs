using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Dyadic.Infrastructure;
using Dyadic.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dyadic.IntegrationTests.E2E;

[Trait("Category", "EndToEnd")]
public class StudentJourneyTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private Guid _researchAreaId;

    public StudentJourneyTests()
    {
        _factory = new TestWebApplicationFactory();
    }

    public async Task InitializeAsync()
    {
        _researchAreaId = await _factory.SeedResearchAreaAsync("Artificial Intelligence");
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Student_SubmitProposal_RedirectsToMyProposal_AndStatusIsSubmitted()
    {
        await _factory.SeedUserAsync("Alice Student", "alice@test.com", "Student");
        var client = await _factory.GetAuthenticatedClientAsync("alice@test.com");

        var getResponse = await client.GetAsync("/Student/SubmitProposal");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var token = TestWebApplicationFactory.ExtractAntiforgeryToken(
            await getResponse.Content.ReadAsStringAsync());

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Title"]                = "Machine Learning in Healthcare",
            ["Input.Abstract"]             = "This research explores the use of machine learning algorithms in healthcare diagnostics and treatment planning.",
            ["Input.TechStack"]            = "Python, TensorFlow",
            ["Input.ResearchAreaId"]       = _researchAreaId.ToString(),
            ["__RequestVerificationToken"] = token
        });

        var response = await client.PostAsync("/Student/SubmitProposal?handler=Submit", form);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/Student/MyProposal");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var proposal = await db.Proposals
            .FirstOrDefaultAsync(p => p.Title == "Machine Learning in Healthcare");
        proposal.Should().NotBeNull();
        proposal!.Status.Should().Be(ProposalStatus.Submitted);
    }

    [Fact]
    public async Task Student_WithdrawProposal_RevertsToDraft()
    {
        await _factory.SeedUserAsync("Bob Student", "bob@test.com", "Student");
        var client = await _factory.GetAuthenticatedClientAsync("bob@test.com");

        var submitGet = await client.GetAsync("/Student/SubmitProposal");
        var submitToken = TestWebApplicationFactory.ExtractAntiforgeryToken(
            await submitGet.Content.ReadAsStringAsync());

        using var submitForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Title"]                = "Blockchain for Supply Chain",
            ["Input.Abstract"]             = "This research investigates blockchain applications in modern supply chain management and logistics systems.",
            ["Input.TechStack"]            = "Solidity, Ethereum",
            ["Input.ResearchAreaId"]       = _researchAreaId.ToString(),
            ["__RequestVerificationToken"] = submitToken
        });
        await client.PostAsync("/Student/SubmitProposal?handler=Submit", submitForm);

        var myProposalGet = await client.GetAsync("/Student/MyProposal");
        myProposalGet.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var withdrawToken = TestWebApplicationFactory.ExtractAntiforgeryToken(
            await myProposalGet.Content.ReadAsStringAsync());

        using var withdrawForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = withdrawToken
        });
        var response = await client.PostAsync("/Student/MyProposal?handler=Withdraw", withdrawForm);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var proposal = await db.Proposals
            .FirstOrDefaultAsync(p => p.Title == "Blockchain for Supply Chain");
        proposal.Should().NotBeNull();
        proposal!.Status.Should().Be(ProposalStatus.Draft);
    }

    [Fact]
    public async Task FullJourney_Register_SubmitProposal_ConfirmMatch_SeesSupervisor()
    {
        // 1. Register + login Student
        await _factory.SeedUserAsync("Carol Student", "carol@test.com", "Student");
        var client = await _factory.GetAuthenticatedClientAsync("carol@test.com");

        // 2. Student submits a proposal via HTTP
        var submitGet = await client.GetAsync("/Student/SubmitProposal");
        var submitToken = TestWebApplicationFactory.ExtractAntiforgeryToken(
            await submitGet.Content.ReadAsStringAsync());

        using var submitForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Title"]                = "Full Journey Research Proposal",
            ["Input.Abstract"]             = "This research explores the end-to-end matching workflow from student submission through supervisor confirmation and identity reveal.",
            ["Input.TechStack"]            = "C#, ASP.NET Core",
            ["Input.ResearchAreaId"]       = _researchAreaId.ToString(),
            ["__RequestVerificationToken"] = submitToken
        });
        var submitResponse = await client.PostAsync("/Student/SubmitProposal?handler=Submit", submitForm);
        submitResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect);

        // 3. Seed a supervisor and advance the proposal to Accepted via the service layer
        //    (supervisor HTTP journey is covered separately in SupervisorJourneyTests)
        var supervisor = await _factory.SeedUserAsync("Dr. Reveal", "reveal@test.com", "Supervisor");
        Guid proposalId;

        using (var arrangeScope = _factory.Services.CreateScope())
        {
            var db = arrangeScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var proposalSvc = arrangeScope.ServiceProvider.GetRequiredService<IProposalService>();

            var supervisorProfile = new SupervisorProfile
            {
                Id = Guid.NewGuid(),
                UserId = supervisor.Id,
                Department = "CS",
                MaxStudents = 3
            };
            db.SupervisorProfiles.Add(supervisorProfile);
            await db.SaveChangesAsync();

            var proposal = await db.Proposals
                .FirstAsync(p => p.Title == "Full Journey Research Proposal");
            proposalId = proposal.Id;

            await proposalSvc.AcceptProposalAsync(proposal.Id, supervisorProfile.Id);
        }

        // 4. Student confirms the match
        var confirmGet = await client.GetAsync("/Student/MyProposal");
        confirmGet.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var confirmToken = TestWebApplicationFactory.ExtractAntiforgeryToken(
            await confirmGet.Content.ReadAsStringAsync());

        using var confirmForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["proposalId"]                 = proposalId.ToString(),
            ["__RequestVerificationToken"] = confirmToken
        });
        var confirmResponse = await client.PostAsync("/Student/MyProposal?handler=Confirm", confirmForm);
        confirmResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect);

        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var finalized = await assertDb.Proposals.FindAsync(proposalId);
        finalized!.Status.Should().Be(ProposalStatus.Finalized);

        // 5. Reveal — GET MyProposal, assert supervisor name appears in the response body
        var revealResponse = await client.GetAsync("/Student/MyProposal");
        revealResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var revealHtml = await revealResponse.Content.ReadAsStringAsync();
        revealHtml.Should().Contain("Dr. Reveal");
    }
}
