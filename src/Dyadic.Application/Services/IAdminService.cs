using Dyadic.Application.DTOs;
using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;

namespace Dyadic.Application.Services;

public interface IAdminService
{
    Task<DashboardStats> GetDashboardStatsAsync();
    Task<List<Proposal>> GetAllProposalsAsync(bool includeDrafts = false);
    Task<List<SupervisorProfile>> GetAllSupervisorsWithCapacityAsync();
    Task<List<SupervisorProfile>> GetAvailableSupervisorsAsync(Guid excludeSupervisorId);
    Task ReassignProposalAsync(Guid proposalId, Guid newSupervisorId, Guid adminUserId, string reason);
    Task UnmatchProposalAsync(Guid proposalId, Guid adminUserId, string reason);
    Task<List<AllocationOverride>> GetAuditLogAsync();
    Task<List<ProposalEvent>> GetProposalEventsAsync();
}
