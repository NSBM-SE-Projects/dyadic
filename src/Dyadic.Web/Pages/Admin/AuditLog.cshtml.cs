using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
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

    public List<AllocationOverride> Entries { get; set; } = new();

    public async Task OnGetAsync()
    {
        Entries = await _adminService.GetAuditLogAsync();
    }
}
