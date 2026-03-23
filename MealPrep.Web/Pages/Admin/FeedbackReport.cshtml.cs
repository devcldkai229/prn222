using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class FeedbackReportModel : PageModel
{
    private readonly IMealFeedbackService _feedbackService;

    public FeedbackReportModel(IMealFeedbackService feedbackService) =>
        _feedbackService = feedbackService;

    public List<MealFeedbackReportDto> Report { get; private set; } = [];
    public int MinRatings { get; private set; } = 3;

    public async Task OnGetAsync(int minRatings = 3)
    {
        MinRatings = minRatings;
        Report = await _feedbackService.GetLowRatedMealsReportAsync(minRatings);
    }

    public async Task<IActionResult> OnGetReportJsonAsync(int minRatings = 3)
    {
        var report = await _feedbackService.GetLowRatedMealsReportAsync(minRatings);
        return new JsonResult(report.Select(m => new
        {
            m.MealId,
            m.MealName,
            m.ImageUrls,
            m.AverageStars,
            m.TotalRatings,
            m.OneStarCount,
            m.TwoStarCount,
        }));
    }
}
