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
        var statusCounts = await _db.Proposals
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        return new DashboardStats
        {
            TotalProposals   = statusCounts.Values.Sum(),
            DraftCount       = statusCounts.GetValueOrDefault(ProposalStatus.Draft),
            SubmittedCount   = statusCounts.GetValueOrDefault(ProposalStatus.Submitted),
            AcceptedCount    = statusCounts.GetValueOrDefault(ProposalStatus.Accepted),
            FinalizedCount   = statusCounts.GetValueOrDefault(ProposalStatus.Finalized),
            TotalStudents    = await _db.StudentProfiles.CountAsync(),
            TotalSupervisors = await _db.SupervisorProfiles.CountAsync()
        };
    }

    public async Task<List<Proposal>> GetAllProposalsAsync(bool includeDrafts = false)
    {
        return await _db.Proposals
            .Where(p => includeDrafts || p.Status != ProposalStatus.Draft)
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

    public async Task<List<SupervisorProfile>> GetAvailableSupervisorsAsync(Guid excludeSupervisorId)
    {
        return await _db.SupervisorProfiles
            .Include(sp => sp.User)
            .Include(sp => sp.AcceptedProposals)
            .Where(sp => sp.Id != excludeSupervisorId &&
                         sp.AcceptedProposals.Count < sp.MaxStudents)
            .OrderBy(sp => sp.User.FullName)
            .ToListAsync();
    }

    public async Task ReassignProposalAsync(Guid proposalId, Guid newSupervisorId, Guid adminUserId, string reason)
    {
        var proposal = await _db.Proposals.FindAsync(proposalId)
            ?? throw new InvalidOperationException("Proposal not found.");

        var newSupervisor = await _db.SupervisorProfiles
            .Include(sp => sp.AcceptedProposals)
            .FirstOrDefaultAsync(sp => sp.Id == newSupervisorId)
            ?? throw new InvalidOperationException("Supervisor not found.");

        if (newSupervisor.AcceptedProposals.Count >= newSupervisor.MaxStudents)
            throw new InvalidOperationException("The selected supervisor is at maximum capacity.");

        var audit = new AllocationOverride
        {
            ProposalId        = proposalId,
            PerformedByUserId = adminUserId,
            Action            = OverrideAction.Reassign,
            OldSupervisorId   = proposal.SupervisorId,
            NewSupervisorId   = newSupervisorId,
            Reason            = reason,
            Timestamp         = DateTime.UtcNow
        };

        proposal.SupervisorId = newSupervisorId;
        proposal.Status       = ProposalStatus.Accepted;

        _db.AllocationOverrides.Add(audit);
        await _db.SaveChangesAsync();
    }

    public async Task UnmatchProposalAsync(Guid proposalId, Guid adminUserId, string reason)
    {
        var proposal = await _db.Proposals.FindAsync(proposalId)
            ?? throw new InvalidOperationException("Proposal not found.");

        var audit = new AllocationOverride
        {
            ProposalId        = proposalId,
            PerformedByUserId = adminUserId,
            Action            = OverrideAction.Unmatch,
            OldSupervisorId   = proposal.SupervisorId,
            NewSupervisorId   = null,
            Reason            = reason,
            Timestamp         = DateTime.UtcNow
        };

        proposal.SupervisorId = null;
        proposal.Status       = ProposalStatus.Submitted;

        _db.AllocationOverrides.Add(audit);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AllocationOverride>> GetAuditLogAsync()
    {
        return await _db.AllocationOverrides
            .Include(a => a.Proposal)
            .Include(a => a.PerformedBy)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}
