using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dyadic.Web.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    public void OnGet() { }
}
