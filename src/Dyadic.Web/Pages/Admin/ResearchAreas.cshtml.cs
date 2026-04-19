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
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string NewName { get; set; } = string.Empty;

    [BindProperty]
    public Guid EditId { get; set; }

    [BindProperty]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string EditName { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        Areas = await _researchAreaService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            Areas = await _researchAreaService.GetAllAsync();
            return Page();
        }

        try
        {
            await _researchAreaService.CreateAsync(NewName);
            TempData["Success"] = $"Research area '{NewName}' added.";
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

        if (!ModelState.IsValid)
        {
            Areas = await _researchAreaService.GetAllAsync();
            return Page();
        }

        try
        {
            await _researchAreaService.UpdateAsync(EditId, EditName);
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
