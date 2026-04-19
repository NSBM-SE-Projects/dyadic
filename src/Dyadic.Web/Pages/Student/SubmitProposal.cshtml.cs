using System.ComponentModel.DataAnnotations;
using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dyadic.Web.Pages.Student;

[Authorize(Roles = "Student")]
public class SubmitProposalModel : PageModel
{
    private readonly IProposalService _proposalService;
    private readonly IResearchAreaService _researchAreaService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubmitProposalModel(IProposalService proposalService, IResearchAreaService researchAreaService, UserManager<ApplicationUser> userManager)
    {
        _proposalService = proposalService;
        _researchAreaService = researchAreaService;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool IsSubmitted { get; set; }
    public bool IsLocked { get; set; }
    public List<SelectListItem> ResearchAreaOptions { get; set; } = new();

    public class InputModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
        [MinLength(10, ErrorMessage = "Title must be at least 10 characters.")]
        [MaxLength(200)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-.,:;()'""?!]+$",
            ErrorMessage = "Title contains invalid characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Abstract is required.")]
        [MinLength(50, ErrorMessage = "Abstract must be at least 50 characters.")]
        [MaxLength(2000)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-.,:;()'""?!\r\n]+$",
            ErrorMessage = "Abstract contains invalid characters.")]
        public string Abstract { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Tech Stack is required.")]
        [MinLength(3, ErrorMessage = "Tech Stack must be at least 3 characters.")]
        [MaxLength(500)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-.,+#/()]+$",
            ErrorMessage = "Tech Stack contains invalid characters.")]
        public string TechStack { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a research area.")]
        public Guid? ResearchAreaId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await _proposalService.GetOrCreateStudentProfileAsync(user.Id);
        var proposal = await _proposalService.GetByStudentIdAsync(profile.Id);

        if (proposal != null)
        {
            Input.Title = proposal.Title;
            Input.Abstract = proposal.Abstract;
            Input.TechStack = proposal.TechStack;
            Input.ResearchAreaId = proposal.ResearchAreaId;
            IsSubmitted = proposal.Status == ProposalStatus.Submitted;
            IsLocked = proposal.Status == ProposalStatus.Accepted || proposal.Status == ProposalStatus.Finalized;

            if (IsLocked)
                return RedirectToPage("/Student/MyProposal");
        }

        await LoadResearchAreas();
        return Page();
    }

    public async Task<IActionResult> OnPostDraftAsync()
    {
        Input.Title = Input.Title.Trim();
        Input.Abstract = Input.Abstract.Trim();
        Input.TechStack = Input.TechStack.Trim();
        ModelState.Clear();
        TryValidateModel(Input, nameof(Input));

        if (!ModelState.IsValid)
        {
            await LoadResearchAreas();
            return Page();
        }

        if (Input.ResearchAreaId.HasValue)
        {
            var areas = await _researchAreaService.GetActiveAsync();
            if (!areas.Any(a => a.Id == Input.ResearchAreaId.Value))
            {
                ModelState.AddModelError("Input.ResearchAreaId", "Invalid research area.");
                await LoadResearchAreas();
                return Page();
            }
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await _proposalService.GetOrCreateStudentProfileAsync(user.Id);
        var existing = await _proposalService.GetByStudentIdAsync(profile.Id);

        if (existing == null)
            await _proposalService.CreateDraftAsync(profile.Id, Input.Title, Input.Abstract, Input.TechStack, Input.ResearchAreaId);
        else
            await _proposalService.UpdateDraftAsync(existing.Id, Input.Title, Input.Abstract, Input.TechStack, Input.ResearchAreaId);

        return RedirectToPage("/Student/MyProposal");
    }

    public async Task<IActionResult> OnPostSubmitAsync()
    {
        Input.Title = Input.Title.Trim();
        Input.Abstract = Input.Abstract.Trim();
        Input.TechStack = Input.TechStack.Trim();
        ModelState.Clear();
        TryValidateModel(Input, nameof(Input));

        if (!ModelState.IsValid)
        {
            await LoadResearchAreas();
            return Page();
        }

        if (Input.ResearchAreaId.HasValue)
        {
            var areas = await _researchAreaService.GetActiveAsync();
            if (!areas.Any(a => a.Id == Input.ResearchAreaId.Value))
            {
                ModelState.AddModelError("Input.ResearchAreaId", "Invalid research area.");
                await LoadResearchAreas();
                return Page();
            }
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var profile = await _proposalService.GetOrCreateStudentProfileAsync(user.Id);
        var existing = await _proposalService.GetByStudentIdAsync(profile.Id);

        if (existing == null)
            existing = await _proposalService.CreateDraftAsync(profile.Id, Input.Title, Input.Abstract, Input.TechStack, Input.ResearchAreaId);
        else
            await _proposalService.UpdateDraftAsync(existing.Id, Input.Title, Input.Abstract, Input.TechStack, Input.ResearchAreaId);

        await _proposalService.SubmitAsync(existing.Id);
        return RedirectToPage("/Student/MyProposal");
    }

    private async Task LoadResearchAreas()
    {
        var areas = await _researchAreaService.GetActiveAsync();
        ResearchAreaOptions = areas.Select(a => new SelectListItem(a.Name, a.Id.ToString())).ToList();
    }
}
