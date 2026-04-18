using Dyadic.Application.DTOs;
using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dyadic.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DashboardModel : PageModel
{
    private readonly IAdminService _adminService;

    public DashboardModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public DashboardStats Stats { get; set; } = new();
    public List<Proposal> Proposals { get; set; } = new();

    public async Task OnGetAsync()
    {
        Stats = await _adminService.GetDashboardStatsAsync();
        Proposals = await _adminService.GetAllProposalsAsync();
    }
}
