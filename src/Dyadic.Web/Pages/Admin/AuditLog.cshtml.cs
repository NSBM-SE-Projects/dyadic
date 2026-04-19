using Dyadic.Application.DTOs;
using Dyadic.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dyadic.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AuditLogModel : PageModel
{
    private readonly IAdminService _adminService;

    public AuditLogModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public List<AuditLogEntry> Entries { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "All";

    public async Task OnGetAsync()
    {
        var allEntries = new List<AuditLogEntry>();

        if (Filter == "All" || Filter == "AdminOverrides")
        {
            var overrides = await _adminService.GetAuditLogAsync();
            allEntries.AddRange(overrides.Select(a => new AuditLogEntry
            {
                Timestamp     = a.Timestamp,
                Actor         = a.PerformedBy.FullName,
                ActorRole     = "Admin",
                Action        = a.Action.ToString(),
                ProposalTitle = a.Proposal.Title,
                OldSupervisor = a.OldSupervisor?.User.FullName,
                NewSupervisor = a.NewSupervisor?.User.FullName,
                Reason        = a.Reason
            }));
        }

        if (Filter == "All" || Filter == "StudentEvents")
        {
            var events = await _adminService.GetProposalEventsAsync();
            allEntries.AddRange(events.Select(e => new AuditLogEntry
            {
                Timestamp     = e.Timestamp,
                Actor         = e.Actor.FullName,
                ActorRole     = "Student",
                Action        = e.EventType.ToString(),
                ProposalTitle = e.Proposal.Title,
                OldSupervisor = e.RelatedSupervisor?.User.FullName,
                NewSupervisor = null,
                Reason        = null
            }));
        }

        Entries = allEntries.OrderByDescending(e => e.Timestamp).ToList();
    }
}
