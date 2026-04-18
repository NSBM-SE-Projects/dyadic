using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dyadic.Web.Pages.Student;

[Authorize(Roles = "Student")]
public class MyProposalModel : PageModel
{
    private readonly IProposalService _proposalService;
    private readonly UserManager<ApplicationUser> _userManager;

    public MyProposalModel(IProposalService proposalService, UserManager<ApplicationUser> userManager)
    {
        _proposalService = proposalService;
        _userManager = userManager;
    }

    public Proposal? Proposal { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await _proposalService.GetOrCreateStudentProfileAsync(user.Id);
        if (profile == null)
            return RedirectToPage("/Student/SubmitProposal");

        Proposal = await _proposalService.GetByStudentIdAsync(profile.Id);

        if (Proposal == null)
            return RedirectToPage("/Student/SubmitProposal");

        return Page();
    }

    public async Task<IActionResult> OnPostWithdrawAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await _proposalService.GetOrCreateStudentProfileAsync(user.Id);
        var proposal = await _proposalService.GetByStudentIdAsync(profile.Id);
        if (proposal == null) return RedirectToPage("/Student/SubmitProposal");

        try
        {
            await _proposalService.WithdrawAsync(proposal.Id, profile.Id);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Proposal = proposal;
            return Page();
        }

        return RedirectToPage("/Student/MyProposal");
    }

    public async Task<IActionResult> OnPostConfirmAsync(Guid proposalId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await _proposalService.GetOrCreateStudentProfileAsync(user.Id);
        await _proposalService.ConfirmMatchAsync(proposalId, profile.Id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid proposalId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await _proposalService.GetOrCreateStudentProfileAsync(user.Id);
        await _proposalService.RejectMatchAsync(proposalId, profile.Id);
        return RedirectToPage();
    }
}
