using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dyadic.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class SupervisorsModel : PageModel
{
    private readonly IAdminService _adminService;

    public SupervisorsModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public List<SupervisorProfile> Supervisors { get; set; } = new();

    public async Task OnGetAsync()
    {
        Supervisors = await _adminService.GetAllSupervisorsWithCapacityAsync();
    }
}
