using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json.Serialization;

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

    [BindProperty(SupportsGet = true, Name = "q1y")]
    public int? CompareQuarter1Year { get; set; }

    [BindProperty(SupportsGet = true, Name = "q1")]
    public int? CompareQuarter1 { get; set; }

    [BindProperty(SupportsGet = true, Name = "q2y")]
    public int? CompareQuarter2Year { get; set; }

    [BindProperty(SupportsGet = true, Name = "q2")]
    public int? CompareQuarter2 { get; set; }

    [BindProperty(SupportsGet = true, Name = "y1")]
    public int? CompareYear1 { get; set; }

    [BindProperty(SupportsGet = true, Name = "y2")]
    public int? CompareYear2 { get; set; }

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

        Dashboard = await _dashboardService.GetDashboardAsync(
            Days,
            FromDate,
            ToDate,
            CompareQuarter1Year,
            CompareQuarter1,
            CompareQuarter2Year,
            CompareQuarter2,
            CompareYear1,
            CompareYear2
        );
        return Page();
    }

    public async Task<IActionResult> OnGetRevenueComparisonAsync(
        int? q1y,
        int? q1,
        int? q2y,
        int? q2,
        int? y1,
        int? y2,
        int days = 30,
        DateOnly? from = null,
        DateOnly? to = null)
    {
        if (days <= 0)
        {
            days = 30;
        }

        if (from.HasValue && to.HasValue && from > to)
        {
            (from, to) = (to, from);
        }

        var dashboard = await _dashboardService.GetDashboardAsync(
            days,
            from,
            to,
            q1y,
            q1,
            q2y,
            q2,
            y1,
            y2
        );

        var payload = new RevenueComparisonPayload
        {
            SelectedQuarter1Year = dashboard.SelectedQuarter1Year,
            SelectedQuarter1 = dashboard.SelectedQuarter1,
            SelectedQuarter2Year = dashboard.SelectedQuarter2Year,
            SelectedQuarter2 = dashboard.SelectedQuarter2,
            SelectedQuarter1Revenue = dashboard.SelectedQuarter1Revenue,
            SelectedQuarter2Revenue = dashboard.SelectedQuarter2Revenue,
            SelectedQuarterRevenueDiffAmount = dashboard.SelectedQuarterRevenueDiffAmount,
            SelectedQuarterRevenueDiffPercent = dashboard.SelectedQuarterRevenueDiffPercent,
            SelectedYear1 = dashboard.SelectedYear1,
            SelectedYear2 = dashboard.SelectedYear2,
            SelectedYear1Revenue = dashboard.SelectedYear1Revenue,
            SelectedYear2Revenue = dashboard.SelectedYear2Revenue,
            SelectedYearRevenueDiffAmount = dashboard.SelectedYearRevenueDiffAmount,
            SelectedYearRevenueDiffPercent = dashboard.SelectedYearRevenueDiffPercent,
        };

        return new JsonResult(payload);
    }

    private sealed class RevenueComparisonPayload
    {
        [JsonPropertyName("selectedQuarter1Year")]
        public int SelectedQuarter1Year { get; set; }

        [JsonPropertyName("selectedQuarter1")]
        public int SelectedQuarter1 { get; set; }

        [JsonPropertyName("selectedQuarter2Year")]
        public int SelectedQuarter2Year { get; set; }

        [JsonPropertyName("selectedQuarter2")]
        public int SelectedQuarter2 { get; set; }

        [JsonPropertyName("selectedQuarter1Revenue")]
        public decimal SelectedQuarter1Revenue { get; set; }

        [JsonPropertyName("selectedQuarter2Revenue")]
        public decimal SelectedQuarter2Revenue { get; set; }

        [JsonPropertyName("selectedQuarterRevenueDiffAmount")]
        public decimal SelectedQuarterRevenueDiffAmount { get; set; }

        [JsonPropertyName("selectedQuarterRevenueDiffPercent")]
        public decimal? SelectedQuarterRevenueDiffPercent { get; set; }

        [JsonPropertyName("selectedYear1")]
        public int SelectedYear1 { get; set; }

        [JsonPropertyName("selectedYear2")]
        public int SelectedYear2 { get; set; }

        [JsonPropertyName("selectedYear1Revenue")]
        public decimal SelectedYear1Revenue { get; set; }

        [JsonPropertyName("selectedYear2Revenue")]
        public decimal SelectedYear2Revenue { get; set; }

        [JsonPropertyName("selectedYearRevenueDiffAmount")]
        public decimal SelectedYearRevenueDiffAmount { get; set; }

        [JsonPropertyName("selectedYearRevenueDiffPercent")]
        public decimal? SelectedYearRevenueDiffPercent { get; set; }
    }
}
