using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.Infrastructure.Services;

public class ProposalService : IProposalService
{
    private readonly ApplicationDbContext _db;

    public ProposalService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<StudentProfile> GetOrCreateStudentProfileAsync(Guid userId)
    {
        var profile = await _db.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == userId);
        if (profile == null)
        {
            profile = new StudentProfile
            {
                UserId = userId,
                IndexNumber = "N/A",
                Batch = "N/A"
            };
            _db.StudentProfiles.Add(profile);
            await _db.SaveChangesAsync();
        }
        return profile;
    }

    public async Task<Proposal> CreateDraftAsync(Guid studentProfileId, string title, string abstractText, string techStack, Guid? researchAreaId)
    {
        if (researchAreaId.HasValue)
        {
            var area = await _db.ResearchAreas.FindAsync(researchAreaId.Value);
            if (area == null || !area.IsActive)
                throw new InvalidOperationException("Selected research area is not available.");
        }

        var proposal = new Proposal
        {
            Title = title,
            Abstract = abstractText,
            TechStack = techStack,
            ResearchAreaId = researchAreaId,
            StudentId = studentProfileId,
            Status = ProposalStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _db.Proposals.Add(proposal);
        await _db.SaveChangesAsync();
        return proposal;
    }

    public async Task<Proposal> UpdateDraftAsync(Guid proposalId, string title, string abstractText, string techStack, Guid? researchAreaId)
    {
        var proposal = await _db.Proposals.FindAsync(proposalId)
            ?? throw new InvalidOperationException("Proposal not found.");

        if (proposal.Status != ProposalStatus.Draft)
            throw new InvalidOperationException("Only draft proposals can be edited.");

        if (researchAreaId.HasValue)
        {
            var area = await _db.ResearchAreas.FindAsync(researchAreaId.Value);
            if (area == null || !area.IsActive)
                throw new InvalidOperationException("Selected research area is not available.");
        }

        proposal.Title = title;
        proposal.Abstract = abstractText;
        proposal.TechStack = techStack;
        proposal.ResearchAreaId = researchAreaId;
        await _db.SaveChangesAsync();
        return proposal;
    }

    public async Task<Proposal> SubmitAsync(Guid proposalId)
    {
        var proposal = await _db.Proposals.FindAsync(proposalId)
            ?? throw new InvalidOperationException("Proposal not found.");

        if (proposal.Status != ProposalStatus.Draft)
            throw new InvalidOperationException("Only draft proposals can be submitted.");

        proposal.Status = ProposalStatus.Submitted;
        await _db.SaveChangesAsync();
        return proposal;
    }

    public async Task<Proposal?> GetByStudentIdAsync(Guid studentProfileId)
    {
        var proposal = await _db.Proposals
            .Include(p => p.ResearchArea)
            .FirstOrDefaultAsync(p => p.StudentId == studentProfileId);

        if (proposal?.Status == ProposalStatus.Finalized)
            await _db.Entry(proposal)
                .Reference(p => p.Supervisor)
                .Query()
                .Include(sp => sp.User)
                .LoadAsync();

        return proposal;
    }

    public async Task<Proposal> WithdrawAsync(Guid proposalId, Guid studentProfileId)
    {
        var proposal = await _db.Proposals.FindAsync(proposalId)
            ?? throw new InvalidOperationException("Proposal not found.");

        if (proposal.StudentId != studentProfileId)
            throw new InvalidOperationException("You do not own this proposal.");

        if (proposal.Status != ProposalStatus.Submitted)
            throw new InvalidOperationException("Only submitted proposals can be withdrawn.");

        proposal.Status = ProposalStatus.Draft;
        await _db.SaveChangesAsync();
        return proposal;
    }

    public async Task<List<Proposal>> GetSubmittedProposalsAsync()
    {
        return await _db.Proposals
            .Where(p => p.Status == ProposalStatus.Submitted)
            .Include(p => p.ResearchArea)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Proposal> AcceptProposalAsync(Guid proposalId, Guid supervisorProfileId)
    {
        var proposal = await _db.Proposals.FindAsync(proposalId)
            ?? throw new InvalidOperationException("Proposal not found.");

        if (proposal.Status != ProposalStatus.Submitted)
            throw new InvalidOperationException("Only submitted proposals can be accepted.");

        var supervisor = await _db.SupervisorProfiles
            .Include(sp => sp.AcceptedProposals)
            .FirstOrDefaultAsync(sp => sp.Id == supervisorProfileId)
            ?? throw new InvalidOperationException("Supervisor profile not found.");

        if (supervisor.AcceptedProposals.Count >= supervisor.MaxStudents)
            throw new InvalidOperationException("You have reached your maximum student capacity.");

        proposal.SupervisorId = supervisorProfileId;
        proposal.Status = ProposalStatus.Accepted;
        await _db.SaveChangesAsync();
        return proposal;
    }

    public async Task<List<Proposal>> GetAcceptedBySupervisorAsync(Guid supervisorProfileId)
    {
        var proposals = await _db.Proposals
            .Where(p => p.SupervisorId == supervisorProfileId &&
                        (p.Status == ProposalStatus.Accepted || p.Status == ProposalStatus.Finalized))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        foreach (var proposal in proposals.Where(p => p.Status == ProposalStatus.Finalized))
            await _db.Entry(proposal)
                .Reference(p => p.Student)
                .Query()
                .Include(sp => sp.User)
                .LoadAsync();

        return proposals;
    }

    public async Task<Proposal> ConfirmMatchAsync(Guid proposalId, Guid studentProfileId)
    {
        var proposal = await _db.Proposals.FindAsync(proposalId)
            ?? throw new InvalidOperationException("Proposal not found.");

        if (proposal.StudentId != studentProfileId)
            throw new InvalidOperationException("You do not own this proposal.");

        if (proposal.Status != ProposalStatus.Accepted)
            throw new InvalidOperationException("Only accepted proposals can be confirmed.");

        proposal.Status = ProposalStatus.Finalized;
        await _db.SaveChangesAsync();
        return proposal;
    }

    public async Task<Proposal> RejectMatchAsync(Guid proposalId, Guid studentProfileId)
    {
        var proposal = await _db.Proposals.FindAsync(proposalId)
            ?? throw new InvalidOperationException("Proposal not found.");

        if (proposal.StudentId != studentProfileId)
            throw new InvalidOperationException("You do not own this proposal.");

        if (proposal.Status != ProposalStatus.Accepted)
            throw new InvalidOperationException("Only accepted proposals can be rejected.");

        proposal.Status = ProposalStatus.Submitted;
        proposal.SupervisorId = null;
        await _db.SaveChangesAsync();
        return proposal;
    }
}
