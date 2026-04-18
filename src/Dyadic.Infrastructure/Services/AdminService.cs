using Dyadic.Application.DTOs;
using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _db;

    public AdminService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var proposals = await _db.Proposals.ToListAsync();

        return new DashboardStats
        {
            TotalProposals   = proposals.Count,
            DraftCount       = proposals.Count(p => p.Status == ProposalStatus.Draft),
            SubmittedCount   = proposals.Count(p => p.Status == ProposalStatus.Submitted),
            AcceptedCount    = proposals.Count(p => p.Status == ProposalStatus.Accepted),
            FinalizedCount   = proposals.Count(p => p.Status == ProposalStatus.Finalized),
            TotalStudents    = await _db.StudentProfiles.CountAsync(),
            TotalSupervisors = await _db.SupervisorProfiles.CountAsync()
        };
    }

    public async Task<List<Proposal>> GetAllProposalsAsync()
    {
        return await _db.Proposals
            .Include(p => p.Student).ThenInclude(sp => sp.User)
            .Include(p => p.Supervisor).ThenInclude(sp => sp!.User)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<SupervisorProfile>> GetAllSupervisorsWithCapacityAsync()
    {
        return await _db.SupervisorProfiles
            .Include(sp => sp.User)
            .Include(sp => sp.AcceptedProposals)
            .OrderBy(sp => sp.User.FullName)
            .ToListAsync();
    }
}
