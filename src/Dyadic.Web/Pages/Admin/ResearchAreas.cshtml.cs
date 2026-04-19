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

    public class AddAreaInput
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2–100 characters.")]
        public string Name { get; set; } = string.Empty;
    }

    public class EditAreaInput
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2–100 characters.")]
        public string Name { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        Areas = await _researchAreaService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAddAsync(AddAreaInput input)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = FirstError();
            return RedirectToPage();
        }

        try
        {
            await _researchAreaService.CreateAsync(input.Name.Trim());
            TempData["Success"] = $"Research area '{input.Name.Trim()}' added.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(EditAreaInput input)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = FirstError();
            return RedirectToPage();
        }

        try
        {
            await _researchAreaService.UpdateAsync(input.Id, input.Name.Trim());
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

    public async Task<IActionResult> OnPostReactivateAsync(Guid id)
    {
        try
        {
            await _researchAreaService.ReactivateAsync(id);
            TempData["Success"] = "Research area reactivated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    private string FirstError()
    {
        return ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault() ?? "Invalid input.";
    }
}
