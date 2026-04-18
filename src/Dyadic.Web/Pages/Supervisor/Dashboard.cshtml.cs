using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dyadic.Web.Pages.Supervisor;

[Authorize(Roles = "Supervisor")]
public class DashboardModel : PageModel
{
    private readonly IProposalService _proposalService;
    private readonly ISupervisorProfileService _supervisorProfileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardModel(
        IProposalService proposalService,
        ISupervisorProfileService supervisorProfileService,
        UserManager<ApplicationUser> userManager)
    {
        _proposalService = proposalService;
        _supervisorProfileService = supervisorProfileService;
        _userManager = userManager;
    }

    public List<Proposal> Proposals { get; set; } = new();
    public int AcceptedCount { get; set; }
    public int MaxStudents { get; set; }
    public bool AtCapacity => AcceptedCount >= MaxStudents;

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        var profile = await _supervisorProfileService.GetOrCreateProfileAsync(user.Id);
        AcceptedCount = profile.AcceptedProposals.Count;
        MaxStudents = profile.MaxStudents;
        Proposals = await _proposalService.GetSubmittedProposalsAsync();
    }

    public async Task<IActionResult> OnPostAsync(Guid proposalId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await _supervisorProfileService.GetOrCreateProfileAsync(user.Id);

        try
        {
            await _proposalService.AcceptProposalAsync(proposalId, profile.Id);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            AcceptedCount = profile.AcceptedProposals.Count;
            MaxStudents = profile.MaxStudents;
            Proposals = await _proposalService.GetSubmittedProposalsAsync();
            return Page();
        }

        return RedirectToPage("/Supervisor/AcceptedProposals");
    }
}
