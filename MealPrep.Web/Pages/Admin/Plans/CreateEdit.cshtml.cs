using BusinessObjects.Entities;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Admin.Plans;

[Authorize(Roles = "Admin")]
public class CreateEditModel : PageModel
{
    private readonly IPlanService _planService;

    public CreateEditModel(IPlanService planService) => _planService = planService;

    [BindProperty]
    public CreatePlanDto Dto { get; set; } = new();

    [BindProperty]
    public int? PlanId { get; set; }
    public bool IsEdit => PlanId.HasValue;

    public async Task OnGetAsync(int? planId)
    {
        PlanId = planId;
        if (planId.HasValue)
        {
            var p = await _planService.GetByIdAsync(planId.Value);
            if (p != null)
                Dto = new CreatePlanDto
                {
                    Name = p.Name,
                    Description = p.Description,
                    DurationDays = p.DurationDays,
                    MealsPerDay = p.MealsPerDay,
                    BasePrice = p.BasePrice,
                    ExtraPrice = p.ExtraPrice,
                    IsActive = p.IsActive,
                };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (PlanId.HasValue)
            await _planService.UpdateAsync(PlanId.Value, Dto);
        else
            await _planService.CreateAsync(Dto);

        TempData["SuccessMessage"] = IsEdit ? "Plan updated." : "Plan created.";
        return RedirectToPage("Index");
    }
}
