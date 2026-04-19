using Dyadic.Application.DTOs;
using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dyadic.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsersModel : PageModel
{
    private readonly IUserAdminService _userAdminService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersModel(IUserAdminService userAdminService, UserManager<ApplicationUser> userManager)
    {
        _userAdminService = userAdminService;
        _userManager = userManager;
    }

    public List<UserListItem> Users { get; set; } = new();
    public Guid CurrentAdminId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string RoleFilter { get; set; } = "All";

    [BindProperty(SupportsGet = true)]
    public string StatusFilter { get; set; } = "All";

    public async Task OnGetAsync()
    {
        CurrentAdminId = Guid.Parse(_userManager.GetUserId(User)!);
        var all = await _userAdminService.GetAllUsersAsync();

        Users = all
            .Where(u => RoleFilter == "All" || u.Role == RoleFilter)
            .Where(u => StatusFilter == "All" ||
                        (StatusFilter == "Active" && u.IsActive) ||
                        (StatusFilter == "Inactive" && !u.IsActive))
            .ToList();
    }

    public async Task<IActionResult> OnPostChangeRoleAsync(Guid userId, string newRole)
    {
        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        try
        {
            await _userAdminService.ChangeRoleAsync(userId, newRole, adminId);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid userId)
    {
        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        try
        {
            await _userAdminService.DeactivateAsync(userId, adminId);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReactivateAsync(Guid userId)
    {
        try
        {
            await _userAdminService.ReactivateAsync(userId);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage();
    }
}
