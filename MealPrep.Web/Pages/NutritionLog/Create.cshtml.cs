using System.Security.Claims;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.NutritionLog;

[Authorize]
public class CreateModel : PageModel
{
    private readonly INutritionLogService _logService;

    public CreateModel(INutritionLogService logService) => _logService = logService;

    public List<MealSelectDto> Meals { get; private set; } = [];

    [BindProperty] public int MealId { get; set; }
    [BindProperty] public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty] public int Quantity { get; set; } = 1;

    [TempData] public string? ErrorMessage { get; set; }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync()
    {
        Meals = await _logService.GetActiveMealsForLogAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (MealId == 0)
        {
            ErrorMessage = "Vui lòng chọn món ăn.";
            Meals = await _logService.GetActiveMealsForLogAsync();
            return Page();
        }

        try
        {
            await _logService.LogMealAsync(UserId, MealId, Date, Quantity);
            TempData["SuccessMessage"] = "Đã thêm vào nhật ký dinh dưỡng!";
            return RedirectToPage("Index", new { date = Date.ToString("yyyy-MM-dd") });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Meals = await _logService.GetActiveMealsForLogAsync();
            return Page();
        }
    }
}
