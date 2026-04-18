using Dyadic.Application.DTOs;
using Dyadic.Domain.Entities;

namespace Dyadic.Application.Services;

public interface IAdminService
{
    Task<DashboardStats> GetDashboardStatsAsync();
    Task<List<Proposal>> GetAllProposalsAsync();
    Task<List<SupervisorProfile>> GetAllSupervisorsWithCapacityAsync();
}
