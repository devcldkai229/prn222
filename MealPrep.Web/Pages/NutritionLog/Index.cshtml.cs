using System.Security.Claims;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.NutritionLog;

[Authorize]
public class IndexModel : PageModel
{
    private readonly INutritionLogService _logService;

    public IndexModel(INutritionLogService logService) => _logService = logService;

    public List<NutritionLogEntryDto> Logs { get; private set; } = [];
    public DateOnly SelectedDate { get; private set; }
    public decimal TotalCalories => Logs.Sum(l => l.Calories);
    public decimal TotalProtein => Logs.Sum(l => l.Protein);
    public decimal TotalCarbs => Logs.Sum(l => l.Carbs);
    public decimal TotalFat => Logs.Sum(l => l.Fat);

    [TempData] public string? SuccessMessage { get; set; }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync(DateOnly? date)
    {
        SelectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        Logs = await _logService.GetLogsByDateAsync(UserId, SelectedDate);
    }
}
