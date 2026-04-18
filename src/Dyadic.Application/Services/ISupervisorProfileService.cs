using Dyadic.Domain.Entities;

namespace Dyadic.Application.Services;

public interface ISupervisorProfileService
{
    Task<SupervisorProfile> GetOrCreateProfileAsync(Guid userId);
    Task<SupervisorProfile> UpdateProfileAsync(Guid userId, string department, string researchAreas, int maxStudents);
}
