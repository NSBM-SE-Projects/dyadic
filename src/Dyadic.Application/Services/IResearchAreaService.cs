using Dyadic.Domain.Entities;

namespace Dyadic.Application.Services;

public interface IResearchAreaService
{
    Task<List<ResearchArea>> GetActiveAsync();
    Task<List<ResearchArea>> GetAllAsync();
    Task<ResearchArea> CreateAsync(string name);
    Task<ResearchArea> UpdateAsync(Guid id, string name);
    Task DeactivateAsync(Guid id);
}
