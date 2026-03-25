using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IAdminDashboardService _dashboardService;

    public IndexModel(IAdminDashboardService dashboardService) =>
        _dashboardService = dashboardService;

    public AdminDashboardViewModel Dashboard { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int Days { get; set; } = 30;

    [BindProperty(SupportsGet = true, Name = "from")]
    public DateOnly? FromDate { get; set; }

    [BindProperty(SupportsGet = true, Name = "to")]
    public DateOnly? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Mode { get; set; } = "overview";

    public async Task<IActionResult> OnGetAsync()
    {
        if (Days <= 0)
        {
            Days = 30;
        }

        if (FromDate.HasValue && ToDate.HasValue && FromDate > ToDate)
        {
            (FromDate, ToDate) = (ToDate, FromDate);
        }

        Mode = (Mode ?? "overview").Trim().ToLowerInvariant();
        if (Mode is not ("overview" or "operations" or "strategic"))
        {
            Mode = "overview";
        }

        Dashboard = await _dashboardService.GetDashboardAsync(Days, FromDate, ToDate);
        return Page();
    }
}
