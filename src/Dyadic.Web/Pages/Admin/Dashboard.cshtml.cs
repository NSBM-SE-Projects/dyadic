using Dyadic.Application.DTOs;
using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Dyadic.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DashboardModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardModel(IAdminService adminService, UserManager<ApplicationUser> userManager)
    {
        _adminService = adminService;
        _userManager = userManager;
    }

    public DashboardStats Stats { get; set; } = new();
    public List<Proposal> Proposals { get; set; } = new();
    public List<SupervisorProfile> AvailableSupervisors { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public bool ShowDrafts { get; set; }

    [BindProperty]
    public Guid ProposalId { get; set; }

    [BindProperty]
    public Guid NewSupervisorId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Reason is required.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 500 characters.")]
    public string Reason { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        Stats = await _adminService.GetDashboardStatsAsync();
        Proposals = await _adminService.GetAllProposalsAsync(ShowDrafts);
        AvailableSupervisors = await _adminService.GetAllSupervisorsWithCapacityAsync();
    }

    public async Task<IActionResult> OnPostReassignAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadPageData();
            return Page();
        }

        try
        {
            var adminId = Guid.Parse(_userManager.GetUserId(User)!);
            await _adminService.ReassignProposalAsync(ProposalId, NewSupervisorId, adminId, Reason);
            TempData["Success"] = "Proposal reassigned successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnmatchAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadPageData();
            return Page();
        }

        try
        {
            var adminId = Guid.Parse(_userManager.GetUserId(User)!);
            await _adminService.UnmatchProposalAsync(ProposalId, adminId, Reason);
            TempData["Success"] = "Proposal unmatched successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    private async Task LoadPageData()
    {
        Stats = await _adminService.GetDashboardStatsAsync();
        Proposals = await _adminService.GetAllProposalsAsync(ShowDrafts);
        AvailableSupervisors = await _adminService.GetAllSupervisorsWithCapacityAsync();
    }
}
