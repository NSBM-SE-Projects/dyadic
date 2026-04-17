using Dyadic.Domain.Entities;

namespace Dyadic.Application.Services;

public interface IProposalSerivices;
{
    Task<Proposal> CreateDraftAsync(Guid studentProfileId, string title, string description);
    Task<Proposal> SubmitAsync(Guid proposalId);
    Task<Proposal> GetbyStudentidAsync(Guide studentProfileId);
}