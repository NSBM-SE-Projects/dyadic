using Dyadic.Domain.Entities;
using Dyadic.Infrastructure;
using Dyadic.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.UnitTests.Services;

public class ResearchAreaServiceTests
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

    private static void SeedAreas(ApplicationDbContext ctx, params (string name, bool active)[] areas)
    {
        foreach (var (name, active) in areas)
        {
            ctx.ResearchAreas.Add(new ResearchArea
            {
                Id = Guid.NewGuid(),
                Name = name,
                IsActive = active,
                CreatedAt = DateTime.UtcNow
            });
        }
        ctx.SaveChanges();
    }

    // ── positive tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveAreas_OrderedByName()
    {
        using var ctx = CreateContext();
        SeedAreas(ctx, ("Zebra Studies", true), ("AI Research", true), ("Old Topic", false));
        var svc = new ResearchAreaService(ctx);

        var result = await svc.GetActiveAsync();

        result.Should().HaveCount(2);
        result.Select(r => r.Name).Should().ContainInOrder("AI Research", "Zebra Studies");
        result.Should().NotContain(r => r.Name == "Old Topic");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllAreas_ActiveAndInactive()
    {
        using var ctx = CreateContext();
        SeedAreas(ctx, ("Active Area", true), ("Inactive Area", false));
        var svc = new ResearchAreaService(ctx);

        var result = await svc.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Active Area");
        result.Should().Contain(r => r.Name == "Inactive Area");
    }

    [Fact]
    public async Task CreateAsync_PersistsNewArea_WithNonEmptyId()
    {
        using var ctx = CreateContext();
        var svc = new ResearchAreaService(ctx);

        var result = await svc.CreateAsync("Machine Learning");

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Machine Learning");
        result.IsActive.Should().BeTrue();
        ctx.ResearchAreas.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_RenamesArea_AndPersists()
    {
        using var ctx = CreateContext();
        SeedAreas(ctx, ("Old Name", true));
        var area = ctx.ResearchAreas.Single();
        var svc = new ResearchAreaService(ctx);

        var result = await svc.UpdateAsync(area.Id, "New Name");

        result.Name.Should().Be("New Name");
        ctx.ResearchAreas.Single().Name.Should().Be("New Name");
    }

    [Fact]
    public async Task DeactivateAsync_FlipsIsActiveToFalse()
    {
        using var ctx = CreateContext();
        SeedAreas(ctx, ("Active Area", true));
        var area = ctx.ResearchAreas.Single();
        var svc = new ResearchAreaService(ctx);

        await svc.DeactivateAsync(area.Id);

        ctx.ResearchAreas.Single().IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ReactivateAsync_FlipsIsActiveToTrue()
    {
        using var ctx = CreateContext();
        SeedAreas(ctx, ("Inactive Area", false));
        var area = ctx.ResearchAreas.Single();
        var svc = new ResearchAreaService(ctx);

        await svc.ReactivateAsync(area.Id);

        ctx.ResearchAreas.Single().IsActive.Should().BeTrue();
    }

    // ── negative tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_DuplicateName_Throws()
    {
        using var ctx = CreateContext();
        SeedAreas(ctx, ("Machine Learning", true));
        var svc = new ResearchAreaService(ctx);

        var act = () => svc.CreateAsync("Machine Learning");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentId_Throws()
    {
        using var ctx = CreateContext();
        var svc = new ResearchAreaService(ctx);

        var act = () => svc.UpdateAsync(Guid.NewGuid(), "Anything");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task UpdateAsync_NameExistsOnDifferentRow_Throws()
    {
        using var ctx = CreateContext();
        SeedAreas(ctx, ("Area A", true), ("Area B", true));
        var areaA = ctx.ResearchAreas.First(r => r.Name == "Area A");
        var svc = new ResearchAreaService(ctx);

        var act = () => svc.UpdateAsync(areaA.Id, "Area B");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task DeactivateAsync_NonExistentId_Throws()
    {
        using var ctx = CreateContext();
        var svc = new ResearchAreaService(ctx);

        var act = () => svc.DeactivateAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}
