using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.Infrastructure.Services;

public class ProposalServices : IProposalSerivices
{
    private readonly ApplicationDbContext _db;

    public ProposalServices(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Proposal> CreateDraftAsync(Guid studentProfileId, string title, string description)
    {
        var proposal = new Proposal
        {
            Title = title,
            Description = description,
            StudentId = studentProfileId,
            Status = ProposalStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _db.Proposals.Add(proposal);
        await _db.SaveChangesAsync();
        return proposal;
    }

    public async Task<Proposal> SubmitAsync(Guid proposalId)
    {
        var proposal = await _db.Proposals.FindAsync(proposalId)
            ?? throw new InvalidOperationException("Proposal not found.");

        if (proposal.Status != ProposalStatus.Draft)
            throw new InvalidOperationException("Only draft proposal can be submited.");

        proposal.Status = ProposalStatus.Submitted;
        await _db.SaveChangesAsync();
        return proposal;
    }

    public async Task<Proposal?> GetByStudentIdAsync(Guid studentProfileId)
    {
        return await _db.Proposals
            .FirstOrDefaultAsync(p => p.StudentId == studentProfileId);
    }
}