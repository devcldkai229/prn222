using MealPrep.BLL.Hubs;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace MealPrep.Web.Pages.Admin.Meals;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IMealService _mealService;
    private readonly IHubContext<MealHub> _mealHub;

    public IndexModel(IMealService mealService, IHubContext<MealHub> mealHub)
    {
        _mealService = mealService;
        _mealHub = mealHub;
    }

    public PagedResult<MealListItemDto> Result { get; private set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;
    public const int PageSize = 12;

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        Result = await _mealService.GetPagedAsync(PageIndex, PageSize, Search, IsActive);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int mealId)
    {
        await _mealService.SoftDeleteAsync(mealId);
        await _mealHub.Clients.All.SendAsync("MealDeleted", mealId);
        SuccessMessage = "Meal deactivated.";
        return RedirectToPage();
    }
}
