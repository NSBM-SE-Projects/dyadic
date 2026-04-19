using Dyadic.Application.DTOs;
using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.Infrastructure.Services;

public class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAdminService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserListItem>> GetAllUsersAsync()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var result = new List<UserListItem>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserListItem
            {
                Id        = user.Id,
                FullName  = user.FullName,
                Email     = user.Email ?? string.Empty,
                Role      = roles.FirstOrDefault() ?? "—",
                IsActive  = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }
        return result;
    }

    public async Task ChangeRoleAsync(Guid userId, string newRole, Guid requestingAdminId)
    {
        if (userId == requestingAdminId)
            throw new InvalidOperationException("You cannot change your own role.");

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found.");

        if (newRole != "Admin")
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count == 1 && admins[0].Id == userId)
                throw new InvalidOperationException("Cannot remove the last administrator.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            throw new InvalidOperationException("Failed to remove existing role.");

        var addResult = await _userManager.AddToRoleAsync(user, newRole);
        if (!addResult.Succeeded)
        {
            // Restore original role on failure
            if (currentRoles.Any())
                await _userManager.AddToRoleAsync(user, currentRoles.First());
            throw new InvalidOperationException("Failed to assign new role.");
        }
    }

    public async Task DeactivateAsync(Guid userId, Guid requestingAdminId)
    {
        if (userId == requestingAdminId)
            throw new InvalidOperationException("You cannot deactivate your own account.");

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found.");

        user.IsActive = false;
        await _userManager.UpdateAsync(user);
    }

    public async Task ReactivateAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found.");

        user.IsActive = true;
        await _userManager.UpdateAsync(user);
    }
}
