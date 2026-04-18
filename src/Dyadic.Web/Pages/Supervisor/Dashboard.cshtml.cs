using Dyadic.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dyadic.Web.Pages.Supervisor;

[Authorize(Roles = "Supervisor")]
public class DashboardModel : PageModel
{
    private readonly ISupervisorProfileService _supervisorProfileService;

    public DashboardModel(ISupervisorProfileService supervisorProfileService)
    {
        _supervisorProfileService = supervisorProfileService;
    }

    public string Department { get; set; } = string.Empty;
    public string ResearchAreas { get; set; } = string.Empty;
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
}
