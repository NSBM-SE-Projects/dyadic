using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dyadic.Web.Pages.Supervisor;

[Authorize(Roles = "Supervisor")]
public class AcceptedProposalsModel : PageModel
{
    private readonly IProposalService _proposalService;
    private readonly ISupervisorProfileService _supervisorProfileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AcceptedProposalsModel(
        IProposalService proposalService,
        ISupervisorProfileService supervisorProfileService,
        UserManager<ApplicationUser> userManager)
    {
        _proposalService = proposalService;
        _supervisorProfileService = supervisorProfileService;
        _userManager = userManager;
    }

    public List<Proposal> Proposals { get; set; } = new();

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        var profile = await _supervisorProfileService.GetOrCreateProfileAsync(user.Id);
        Proposals = await _proposalService.GetAcceptedBySupervisorAsync(profile.Id);
    }
}
