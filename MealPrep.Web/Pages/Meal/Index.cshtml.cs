using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Meal;

public class IndexModel : PageModel
{
    private readonly IMealService _mealService;

    public IndexModel(IMealService mealService) => _mealService = mealService;

    public PagedResult<MealListItemDto> Result { get; private set; } = null!;

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;
    public const int PageSize = 12;

    public async Task OnGetAsync()
    {
        Result = await _mealService.SearchWithPaginationAsync(Q, PageIndex, PageSize);
    }
}
