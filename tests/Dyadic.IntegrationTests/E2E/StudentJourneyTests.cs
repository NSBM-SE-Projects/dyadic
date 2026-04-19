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

        // GET submit page to obtain antiforgery token
        var getResponse = await client.GetAsync("/Student/SubmitProposal");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var token = TestWebApplicationFactory.ExtractAntiforgeryToken(
            await getResponse.Content.ReadAsStringAsync());

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
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

        // Submit a proposal first
        var submitGet = await client.GetAsync("/Student/SubmitProposal");
        var submitToken = TestWebApplicationFactory.ExtractAntiforgeryToken(
            await submitGet.Content.ReadAsStringAsync());

        await client.PostAsync("/Student/SubmitProposal?handler=Submit",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Title"]                = "Blockchain for Supply Chain",
                ["Input.Abstract"]             = "This research investigates blockchain applications in modern supply chain management and logistics systems.",
                ["Input.TechStack"]            = "Solidity, Ethereum",
                ["Input.ResearchAreaId"]       = _researchAreaId.ToString(),
                ["__RequestVerificationToken"] = submitToken
            }));

        // GET MyProposal to obtain withdraw antiforgery token
        var myProposalGet = await client.GetAsync("/Student/MyProposal");
        myProposalGet.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var withdrawToken = TestWebApplicationFactory.ExtractAntiforgeryToken(
            await myProposalGet.Content.ReadAsStringAsync());

        var response = await client.PostAsync("/Student/MyProposal?handler=Withdraw",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = withdrawToken
            }));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var proposal = await db.Proposals
            .FirstOrDefaultAsync(p => p.Title == "Blockchain for Supply Chain");
        proposal.Should().NotBeNull();
        proposal!.Status.Should().Be(ProposalStatus.Draft);
    }
}
