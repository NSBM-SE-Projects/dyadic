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

    public SubmitProposalModel(IProposalService proposalService, UserManager<ApplicationUser> userManager)
    {
        _proposalService = proposalService;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool IsSubmitted { get; set; }
    public bool IsLocked { get; set; }
    public class InputModel
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Description { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        var profile = await _proposalService.GetOrCreateStudentProfileAsync(user.Id);
        var proposal = await _proposalService.GetByStudentIdAsync(profile.Id);

        if (proposal != null)
        {
            Input.Title = proposal.Title;
            Input.Description = proposal.Description;
            IsSubmitted = proposal.Status == ProposalStatus.Submitted;
            IsLocked = proposal.Status == ProposalStatus.Accepted || proposal.Status == ProposalStatus.Finalized;

            if (IsLocked)
            {
                Response.Redirect("/Student/MyProposal");
                return;
            }
        }
    }

    public async Task<IActionResult> OnPostDraftAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await _proposalService.GetOrCreateStudentProfileAsync(user.Id);
        var existing = await _proposalService.GetByStudentIdAsync(profile.Id);

        if (existing == null)
            await _proposalService.CreateDraftAsync(profile.Id, Input.Title, Input.Description);
        else
            await _proposalService.UpdateDraftAsync(existing.Id, Input.Title, Input.Description);

        return RedirectToPage("/Student/MyProposal");
    }

    public async Task<IActionResult> OnPostSubmitAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await _proposalService.GetOrCreateStudentProfileAsync(user.Id);
        var existing = await _proposalService.GetByStudentIdAsync(profile.Id);

        if (existing == null)
            existing = await _proposalService.CreateDraftAsync(profile.Id, Input.Title, Input.Description);

        await _proposalService.SubmitAsync(existing.Id);
        return RedirectToPage("/Student/MyProposal");
    }
}
