using Dyadic.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Dyadic.Web.Pages.Supervisor;

[Authorize(Roles = "Supervisor")]
public class ProfileModel : PageModel
{
    private readonly ISupervisorProfileService _supervisorProfileService;

    public ProfileModel(ISupervisorProfileService supervisorProfileService)
    {
        _supervisorProfileService = supervisorProfileService;
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
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var profile = await _supervisorProfileService.GetOrCreateProfileAsync(userId);

        Department = profile.Department;
        ResearchAreas = profile.ResearchAreas;
        MaxStudents = profile.MaxStudents;
        AcceptedCount = profile.AcceptedProposals.Count;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var profile = await _supervisorProfileService.GetOrCreateProfileAsync(userId);
            AcceptedCount = profile.AcceptedProposals.Count;
            return Page();
        }

        try
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            await _supervisorProfileService.UpdateProfileAsync(userId, Department, ResearchAreas, MaxStudents);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(MaxStudents), ex.Message);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var profile = await _supervisorProfileService.GetOrCreateProfileAsync(userId);
            AcceptedCount = profile.AcceptedProposals.Count;
            return Page();
        }
    }
}
