using Dyadic.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.IntegrationTests.SqlServer;

[Trait("Category", "SqlServer")]
public class MigrationTests : IAsyncLifetime
{
    private readonly SqlServerDbContextFactory _factory = new();
    private Infrastructure.ApplicationDbContext _db = null!;

    public async Task InitializeAsync() => _db = await _factory.CreateAsync();
    public async Task DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task AllMigrations_ApplyCleanly_FromScratch()
    {
        // If InitializeAsync completed without throwing, migrations applied cleanly.
        // Verify by checking pending migrations — should be empty.
        var pending = await _db.Database.GetPendingMigrationsAsync();
        pending.Should().BeEmpty();
    }

    [Fact]
    public async Task SeededResearchAreas_Count_Is14()
    {
        var count = await _db.ResearchAreas.CountAsync();
        count.Should().Be(14);
    }

    [Fact]
    public async Task SeededResearchAreas_HaveFullNames_NotAbbreviations()
    {
        var names = await _db.ResearchAreas.Select(r => r.Name).ToListAsync();

        names.Should().Contain("Artificial Intelligence");
        names.Should().Contain("Human-Computer Interaction");
        names.Should().Contain("Natural Language Processing");
        names.Should().Contain("Internet of Things");

        names.Should().NotContain("AI");
        names.Should().NotContain("HCI");
        names.Should().NotContain("NLP");
        names.Should().NotContain("IoT");
    }
}
