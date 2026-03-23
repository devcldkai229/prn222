using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Meal;

public class DetailModel : PageModel
{
    private readonly IMealService _mealService;

    public DetailModel(IMealService mealService) => _mealService = mealService;

    public MealListItemDto Meal { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var meal = await _mealService.GetByIdAsync(id);
        if (meal == null)
            return NotFound();
        Meal = meal;
        return Page();
    }
}
