using Dyadic.Domain.Entities;

namespace Dyadic.Application.Services;

public interface IProposalService
{
    Task<Proposal> CreateDraftAsync(Guid studentProfileId, string title, string description);
    Task<Proposal> SubmitAsync(Guid proposalId);
    Task<Proposal?> GetByStudentIdAsync(Guid studentProfileId);
}
