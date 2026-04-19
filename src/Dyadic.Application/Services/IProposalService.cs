using Dyadic.Domain.Entities;

namespace Dyadic.Application.Services;

public interface IProposalService
{
    Task<StudentProfile> GetOrCreateStudentProfileAsync(Guid userId);
    Task<Proposal> CreateDraftAsync(Guid studentProfileId, string title, string abstractText, string techStack, Guid? researchAreaId);
    Task<Proposal> UpdateDraftAsync(Guid proposalId, string title, string abstractText, string techStack, Guid? researchAreaId);
    Task<Proposal> SubmitAsync(Guid proposalId);
    Task<Proposal?> GetByStudentIdAsync(Guid studentProfileId);
    Task<Proposal> WithdrawAsync(Guid proposalId, Guid studentProfileId, Guid actorUserId);
    Task<List<Proposal>> GetSubmittedProposalsAsync(Guid? researchAreaId = null, string sort = "Newest");
    Task<Proposal> AcceptProposalAsync(Guid proposalId, Guid supervisorProfileId);
    Task<List<Proposal>> GetAcceptedBySupervisorAsync(Guid supervisorProfileId);
    Task<Proposal> ConfirmMatchAsync(Guid proposalId, Guid studentProfileId, Guid actorUserId);
    Task<Proposal> RejectMatchAsync(Guid proposalId, Guid studentProfileId, Guid actorUserId);
}
