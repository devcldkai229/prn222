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

    [BindProperty(SupportsGet = true)]
    public string Mode { get; set; } = "overview";

    public async Task<IActionResult> OnGetAsync()
    {
        if (Days is not (7 or 30 or 90))
        {
            Days = 30;
        }

        Mode = (Mode ?? "overview").Trim().ToLowerInvariant();
        if (Mode is not ("overview" or "operations" or "strategic"))
        {
            Mode = "overview";
        }

        Dashboard = await _dashboardService.GetDashboardAsync(Days);
        return Page();
    }
}
