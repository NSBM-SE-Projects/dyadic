using Dyadic.Application.DTOs;

namespace Dyadic.Application.Services;

public interface IUserAdminService
{
    Task<List<UserListItem>> GetAllUsersAsync();
    Task ChangeRoleAsync(Guid userId, string newRole, Guid requestingAdminId);
    Task DeactivateAsync(Guid userId, Guid requestingAdminId);
    Task ReactivateAsync(Guid userId);
}
