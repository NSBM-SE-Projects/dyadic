using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Dyadic.Web.Pages.Supervisor;

[Authorize(Roles = "Supervisor")]
public class ProfileModel : PageModel
{
    private readonly ISupervisorProfileService _supervisorProfileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileModel(ISupervisorProfileService supervisorProfileService, UserManager<ApplicationUser> userManager)
    {
        _supervisorProfileService = supervisorProfileService;
        _userManager = userManager;
    }

    [BindProperty]
    [Required]
    [StringLength(200)]
    public string Department { get; set; } = string.Empty;

    [BindProperty]
    [StringLength(2000)]
    public string ResearchAreas { get; set; } = string.Empty;

    [BindProperty]
    [Range(1, 10)]
    public int MaxStudents { get; set; }

    public int AcceptedCount { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        var profile = await _supervisorProfileService.GetOrCreateProfileAsync(user.Id);
        Department = profile.Department;
        ResearchAreas = profile.ResearchAreas;
        MaxStudents = profile.MaxStudents;
        AcceptedCount = profile.AcceptedProposals.Count;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        if (!ModelState.IsValid)
        {
            var profile = await _supervisorProfileService.GetOrCreateProfileAsync(user.Id);
            AcceptedCount = profile.AcceptedProposals.Count;
            return Page();
        }

        try
        {
            await _supervisorProfileService.UpdateProfileAsync(user.Id, Department, ResearchAreas, MaxStudents);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(MaxStudents), ex.Message);
            var profile = await _supervisorProfileService.GetOrCreateProfileAsync(user.Id);
            AcceptedCount = profile.AcceptedProposals.Count;
            return Page();
        }
    }
}
