using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.Infrastructure.Services;

public class SupervisorProfileService : ISupervisorProfileService
{
    private readonly ApplicationDbContext _db;

    public SupervisorProfileService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<SupervisorProfile> GetOrCreateProfileAsync(Guid userId)
    {
        var profile = await _db.SupervisorProfiles
            .Include(sp => sp.AcceptedProposals)
            .FirstOrDefaultAsync(sp => sp.UserId == userId);

        if (profile == null)
        {
            profile = new SupervisorProfile
            {
                UserId = userId,
                Department = "N/A",
                ResearchAreas = string.Empty,
                MaxStudents = 3
            };
            _db.SupervisorProfiles.Add(profile);
            await _db.SaveChangesAsync();
        }

        return profile;
    }

    public async Task<SupervisorProfile> UpdateProfileAsync(Guid userId, string department, string researchAreas, int maxStudents)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        if (maxStudents < profile.AcceptedProposals.Count)
            throw new InvalidOperationException(
                $"Cannot set MaxStudents to {maxStudents} — you have already accepted {profile.AcceptedProposals.Count} student(s).");

        profile.Department = department;
        profile.ResearchAreas = researchAreas;
        profile.MaxStudents = maxStudents;

        await _db.SaveChangesAsync();
        return profile;
    }
}
