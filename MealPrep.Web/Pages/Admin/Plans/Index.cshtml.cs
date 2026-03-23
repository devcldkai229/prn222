using BusinessObjects.Entities;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Admin.Plans;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IPlanService _planService;

    public IndexModel(IPlanService planService) => _planService = planService;

    public List<Plan> Plans { get; private set; } = [];

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync() => Plans = await _planService.GetAllAsync();

    public async Task<IActionResult> OnPostToggleAsync(int planId)
    {
        var (success, error) = await _planService.ToggleActiveAsync(planId);
        if (success)
            SuccessMessage = "Plan status updated.";
        else
            ErrorMessage = error;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int planId)
    {
        // Uses toggle (soft delete only — same guard applies)
        var (success, error) = await _planService.ToggleActiveAsync(planId);
        if (!success)
            ErrorMessage = error;
        return RedirectToPage();
    }
}
