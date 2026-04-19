using Dyadic.Application.DTOs;
using Dyadic.Domain.Entities;

namespace Dyadic.Application.Services;

public interface IAdminService
{
    Task<DashboardStats> GetDashboardStatsAsync();
    Task<List<Proposal>> GetAllProposalsAsync(bool includeDrafts = false);
    Task<List<SupervisorProfile>> GetAllSupervisorsWithCapacityAsync();
}
