using System.ComponentModel.DataAnnotations;
using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dyadic.Web.Pages.Student;

[Authorize(Roles = "Student")]
public class SubmitProposalModel : PageModel
{
    private readonly IProposalService _proposalService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Infrastructure.ApplicationDbContext _db;

    public SubmitProposalModel(IProposalService proposalService, UserManager<ApplicationUser> userManager, Infrastructure.ApplicationDbContext db)
    {
        _proposalService = proposalService;
        _userManager = userManager;
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool IsSubmitted { get; set; }

    public class InputModel
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Description { get; set; } = string.Empty;
    }

    private async Task<StudentProfile> GetOrCreateStudentProfileAsync(ApplicationUser user)
    {
        var profile = await _db.StudentProfiles.FindAsync(user.Id);
        if (profile == null)
        {
            profile = new StudentProfile
            {
                UserId = user.Id,
                IndexNumber = "N/A",
                Batch = "N/A"
            };
            _db.StudentProfiles.Add(profile);
            await _db.SaveChangesAsync();
        }
        return profile;
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        var profile = await GetOrCreateStudentProfileAsync(user);
        var proposal = await _proposalService.GetByStudentIdAsync(profile.Id);

        if (proposal != null)
        {
            Input.Title = proposal.Title;
            Input.Description = proposal.Description;
            IsSubmitted = proposal.Status != ProposalStatus.Draft;
        }
    }

    public async Task<IActionResult> OnPostDraftAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await GetOrCreateStudentProfileAsync(user);
        var existing = await _proposalService.GetByStudentIdAsync(profile.Id);

        if (existing == null)
            await _proposalService.CreateDraftAsync(profile.Id, Input.Title, Input.Description);

        return RedirectToPage("/Student/MyProposal");
    }

    public async Task<IActionResult> OnPostSubmitAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await GetOrCreateStudentProfileAsync(user);
        var existing = await _proposalService.GetByStudentIdAsync(profile.Id);

        if (existing == null)
            existing = await _proposalService.CreateDraftAsync(profile.Id, Input.Title, Input.Description);

        await _proposalService.SubmitAsync(existing.Id);
        return RedirectToPage("/Student/MyProposal");
    }
}
