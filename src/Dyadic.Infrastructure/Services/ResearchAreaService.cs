using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.Infrastructure.Services;

public class ResearchAreaService : IResearchAreaService
{
    private readonly ApplicationDbContext _db;

    public ResearchAreaService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<ResearchArea>> GetActiveAsync()
    {
        return await _db.ResearchAreas
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<List<ResearchArea>> GetAllAsync()
    {
        return await _db.ResearchAreas
            .Include(r => r.Proposals)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<ResearchArea> CreateAsync(string name)
    {
        var exists = await _db.ResearchAreas.AnyAsync(r => r.Name == name);
        if (exists)
            throw new InvalidOperationException($"A research area named '{name}' already exists.");

        var area = new ResearchArea
        {
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.ResearchAreas.Add(area);
        await _db.SaveChangesAsync();
        return area;
    }

    public async Task<ResearchArea> UpdateAsync(Guid id, string name)
    {
        var area = await _db.ResearchAreas.FindAsync(id)
            ?? throw new InvalidOperationException("Research area not found.");

        var exists = await _db.ResearchAreas.AnyAsync(r => r.Name == name && r.Id != id);
        if (exists)
            throw new InvalidOperationException($"A research area named '{name}' already exists.");

        area.Name = name;
        await _db.SaveChangesAsync();
        return area;
    }

    public async Task DeactivateAsync(Guid id)
    {
        var area = await _db.ResearchAreas.FindAsync(id)
            ?? throw new InvalidOperationException("Research area not found.");

        area.IsActive = false;
        await _db.SaveChangesAsync();
    }

    public async Task ReactivateAsync(Guid id) {
        var area = await _db.ResearchAreas.FindAsync(id)
            ?? throw new InvalidOperationException("Research area not found.");

        area.IsActive = true;
        await _db.SaveChangesAsync();
    }
}
