using Dyadic.Domain.Entities;

namespace Dyadic.Application.Services;

public interface IProposalService
{
    Task<StudentProfile> GetOrCreateStudentProfileAsync(Guid userId);
    Task<Proposal> CreateDraftAsync(Guid studentProfileId, string title, string description);
    Task<Proposal> UpdateDraftAsync(Guid proposalId, string title, string description);
    Task<Proposal> SubmitAsync(Guid proposalId);
    Task<Proposal?> GetByStudentIdAsync(Guid studentProfileId);
}
