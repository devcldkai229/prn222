using System.Security.Claims;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Feedback;

[Authorize]
public class MySummaryModel : PageModel
{
    private readonly IMealFeedbackService _feedbackService;

    public MySummaryModel(IMealFeedbackService feedbackService) =>
        _feedbackService = feedbackService;

    public UserFeedbackSummaryDto Summary { get; private set; } = new();

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync()
    {
        Summary = await _feedbackService.GetUserFeedbackSummaryAsync(UserId);
    }
}
