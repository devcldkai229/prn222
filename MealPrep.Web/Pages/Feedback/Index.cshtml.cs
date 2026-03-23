using System.Security.Claims;
using MealPrep.BLL.Services;
using MealPrep.BLL.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace MealPrep.Web.Pages.Feedback;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IMealFeedbackService _feedbackService;
    private readonly IHubContext<MealHub> _mealHub;
    private readonly IHubContext<DashboardHub> _dashboardHub;
    private readonly IHubContext<OrderTrackingHub> _trackingHub;

    public IndexModel(
        IMealFeedbackService feedbackService,
        IHubContext<MealHub> mealHub,
        IHubContext<DashboardHub> dashboardHub,
        IHubContext<OrderTrackingHub> trackingHub)
    {
        _feedbackService = feedbackService;
        _mealHub = mealHub;
        _dashboardHub = dashboardHub;
        _trackingHub = trackingHub;
    }

    public List<PendingFeedbackDto> PendingFeedbacks { get; private set; } = [];

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync()
    {
        PendingFeedbacks = await _feedbackService.GetPendingFeedbacksAsync(UserId);
    }

    public async Task<IActionResult> OnPostSubmitRatingAsync([FromBody] SubmitRatingDto dto)
    {
        if (dto.Stars < 1 || dto.Stars > 5)
            return new JsonResult(new { success = false, error = "Invalid star rating." });

        await _feedbackService.SubmitRatingAsync(UserId, dto);
        var newAvgRating = await _feedbackService.GetMealAvgRatingAsync(dto.MealId);
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "User";

        // Broadcast new rating to admin & meal displays (realtime update stars)
        await _mealHub.Clients.All.SendAsync("ReceiveNewRating", new
        {
            mealId = dto.MealId,
            stars = dto.Stars,
            tags = dto.Tags,
            ratedBy = userName,
            newAvgRating
        });
        await _dashboardHub.Clients.Group("AdminDashboard").SendAsync("DashboardDataChanged",
            new { type = "NewRating", mealId = dto.MealId, stars = dto.Stars, newAvgRating });
        await _dashboardHub.Clients.Group($"UserDashboard_{UserId}".ToLowerInvariant()).SendAsync("DashboardDataChanged",
            new { type = "NewRating", mealId = dto.MealId, stars = dto.Stars, newAvgRating });
        await _trackingHub.Clients.Group("Tracking_Admins").SendAsync("MealRated", new
        {
            mealId = dto.MealId,
            orderItemId = dto.OrderItemId,
            stars = dto.Stars,
            newAvgRating,
            ratedBy = userName
        });
        await _trackingHub.Clients.Group($"Tracking_User_{UserId}").SendAsync("OrderItemRated", dto.OrderItemId);

        return new JsonResult(
            new
            {
                success = true,
                message = dto.Stars <= 2
                    ? "Thank you! We've noted your feedback and won't suggest this meal again."
                    : "Thank you for your feedback!",
            }
        );
    }
}
