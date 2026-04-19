using System.ComponentModel.DataAnnotations;
using Dyadic.Application.Services;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dyadic.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ResearchAreasModel : PageModel
{
    private readonly IResearchAreaService _researchAreaService;

    public ResearchAreasModel(IResearchAreaService researchAreaService)
    {
        _researchAreaService = researchAreaService;
    }

    public List<ResearchArea> Areas { get; set; } = new();

    [BindProperty]
    public string NewName { get; set; } = string.Empty;

    [BindProperty]
    public Guid EditId { get; set; }

    [BindProperty]
    public string EditName { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        Areas = await _researchAreaService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName))
        {
            TempData["Error"] = "Name is required.";
            return RedirectToPage();
        }

        try
        {
            await _researchAreaService.CreateAsync(NewName.Trim());
            TempData["Success"] = $"Research area '{NewName.Trim()}' added.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            TempData["Error"] = "Name is required.";
            return RedirectToPage();
        }

        try
        {
            await _researchAreaService.UpdateAsync(EditId, EditName.Trim());
            TempData["Success"] = "Research area updated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid id)
    {
        try
        {
            await _researchAreaService.DeactivateAsync(id);
            TempData["Success"] = "Research area deactivated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }
}
