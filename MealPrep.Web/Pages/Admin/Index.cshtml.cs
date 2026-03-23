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

    public async Task<IActionResult> OnGetAsync()
    {
        Dashboard = await _dashboardService.GetDashboardAsync();
        return Page();
    }
}
