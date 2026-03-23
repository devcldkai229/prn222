using System.Security.Claims;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Dashboard;

[Authorize]
public class NutritionSummaryModel : PageModel
{
    private readonly INutritionLogService _nutritionLogService;

    public NutritionSummaryModel(INutritionLogService nutritionLogService) =>
        _nutritionLogService = nutritionLogService;

    public WeeklyNutritionSummary Summary { get; private set; } = null!;
    public DateOnly WeekStart { get; private set; }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync(DateOnly? weekStart)
    {
        // Default to current week Monday
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        WeekStart = weekStart ?? today.AddDays(-(int)today.DayOfWeek + 1);
        Summary = await _nutritionLogService.GetWeeklySummaryAsync(UserId, WeekStart);
    }
}
