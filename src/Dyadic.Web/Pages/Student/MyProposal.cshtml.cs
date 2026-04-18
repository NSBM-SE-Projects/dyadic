using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Dyadic.Web.Pages.Student;

[Authorize(Roles = "Student")]
public class MyProposalModel : PageModel
{
    private readonly IProposalService _proposalService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Infrastructure.ApplicationDbContext _db;

    public MyProposalModel(IProposalService proposalService, UserManager<ApplicationUser> userManager, Infrastructure.ApplicationDbContext db)
    {
        _proposalService = proposalService;
        _userManager = userManager;
        _db = db;
    }

    public Proposal? Proposal { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await _db.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == user.Id);
        if (profile == null)
            return RedirectToPage("/Student/SubmitProposal");

        Proposal = await _proposalService.GetByStudentIdAsync(profile.Id);

        if (Proposal == null)
            return RedirectToPage("/Student/SubmitProposal");

        return Page();
    }
}