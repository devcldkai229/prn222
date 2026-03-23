using System.Security.Claims;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Menu;

[Authorize]
public class SelectMealsModel : PageModel
{
    private readonly IMenuService _menuService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IAiMenuService _aiMenuService;
    private readonly IUserService _userService;

    public SelectMealsModel(
        IMenuService menuService,
        ISubscriptionService subscriptionService,
        IAiMenuService aiMenuService,
        IUserService userService
    )
    {
        _menuService = menuService;
        _subscriptionService = subscriptionService;
        _aiMenuService = aiMenuService;
        _userService = userService;
    }

    public List<MealSummaryDto> AvailableMeals { get; private set; } = [];
    public List<(DateOnly Date, List<MealSummaryDto> Meals)> WeekSelections { get; private set; } = [];
    public DateOnly SubscriptionStart { get; private set; }
    public DateOnly SubscriptionEnd { get; private set; }
    public int MealsPerDay { get; private set; } = 1;
    public DateOnly ViewStart { get; private set; }
    public bool HasDeliveryAddress { get; private set; }
    public int? SubscriptionId { get; private set; }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> OnGetAsync(DateOnly? weekStart)
    {
        var sub = await _subscriptionService.GetActiveSubscriptionAsync(UserId);
        if (sub == null)
            return RedirectToPage("/Subscription/Plans");

        SubscriptionId = sub.Id;
        SubscriptionStart = sub.StartDate;
        SubscriptionEnd = sub.EndDate ?? sub.StartDate.AddDays(sub.Plan?.DurationDays ?? 30);
        MealsPerDay = Math.Clamp(sub.MealsPerDay, 1, 3);

        var profile = await _userService.GetUserProfileAsync(UserId);
        HasDeliveryAddress = !string.IsNullOrWhiteSpace(profile.DeliveryAddress);

        // View window: 7 days, clamp to subscription range
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var suggestedStart = weekStart ?? (today >= SubscriptionStart && today <= SubscriptionEnd ? today : SubscriptionStart);
        if (suggestedStart < SubscriptionStart)
            ViewStart = SubscriptionStart;
        else if (suggestedStart > SubscriptionEnd)
            ViewStart = SubscriptionEnd.DayNumber - SubscriptionStart.DayNumber >= 6 ? SubscriptionEnd.AddDays(-6) : SubscriptionStart;
        else
            ViewStart = suggestedStart;

        var viewEnd = ViewStart.AddDays(6);
        if (viewEnd > SubscriptionEnd)
            viewEnd = SubscriptionEnd;

        var allergiesClaim = User.FindFirstValue("Allergies") ?? "";
        var allergies = allergiesClaim.Split(',', StringSplitOptions.RemoveEmptyEntries);

        AvailableMeals = await _menuService.GetActiveMealsAsync(allergies);
        WeekSelections = await _menuService.GetSelectionsForDateRangeAsync(UserId, ViewStart, viewEnd, SubscriptionId);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveDayAsync([FromBody] SaveDayRequestModel request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (request.Date <= today)
            return new JsonResult(new { success = false, error = "Không thể thay đổi món đã chọn. Thời hạn chỉnh sửa là trước 00:00 ngày giao hàng." });

        var profile = await _userService.GetUserProfileAsync(UserId);
        if (string.IsNullOrWhiteSpace(profile.DeliveryAddress))
            return new JsonResult(new { success = false, error = "Vui lòng cập nhật địa chỉ giao hàng trước khi lưu thực đơn." });

        var sub = await _subscriptionService.GetActiveSubscriptionAsync(UserId);
        if (sub == null)
            return new JsonResult(new { success = false, error = "Không tìm thấy gói đăng ký đang hoạt động." });

        // Bữa 1 → Morning (1), Bữa 2 → Evening (3), Bữa 3 → Afternoon (2)
        var slotMap = new[] { 1, 3, 2 };
        var dto = new SaveDaySelectionsDto
        {
            Date = request.Date,
            Selections = request
                .MealIds.Select((id, index) => new SlotSelection
                {
                    MealId = id,
                    DeliverySlotId = index < slotMap.Length ? slotMap[index] : 1,
                })
                .ToList(),
        };

        try
        {
            await _menuService.SaveDaySelectionsAsync(UserId, sub.Id, dto);
            return new JsonResult(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostClearDayAsync([FromBody] ClearDayRequestModel request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (request.Date <= today)
            return new JsonResult(new { success = false, error = "Không thể xóa món của ngày đã qua." });

        var sub = await _subscriptionService.GetActiveSubscriptionAsync(UserId);
        if (sub == null)
            return new JsonResult(new { success = false, error = "Không tìm thấy gói đăng ký." });

        var dto = new SaveDaySelectionsDto { Date = request.Date, Selections = [] };
        try
        {
            await _menuService.SaveDaySelectionsAsync(UserId, sub.Id, dto);
            return new JsonResult(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }

    public class SaveDayRequestModel
    {
        public DateOnly Date { get; set; }
        public List<int> MealIds { get; set; } = [];
        public int SlotId { get; set; } = 1;
    }

    public class ClearDayRequestModel
    {
        public DateOnly Date { get; set; }
    }

    public class GenerateAiRequest
    {
        public DateOnly WeekStart { get; set; }
        public string? WeeklyNotes { get; set; }
    }

    public async Task<IActionResult> OnPostGenerateAiAsync([FromBody] GenerateAiRequest request)
    {
        var sub = await _subscriptionService.GetActiveSubscriptionAsync(UserId);
        if (sub == null)
            return new JsonResult(new { success = false, error = "No active subscription." });

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = request.WeekStart;
        var weekEnd = weekStart.AddDays(6);
        var endDate = sub.EndDate ?? sub.StartDate.AddDays(sub.Plan?.DurationDays ?? 30);

        // Cho phép đề xuất cho tất cả các ngày chưa tới ngày giao (>= today), kể cả khi đã có thực đơn
        var remainingDates = new List<DateOnly>();
        for (var d = weekStart; d <= weekEnd && d <= endDate; d = d.AddDays(1))
        {
            if (d >= today && d >= sub.StartDate)
                remainingDates.Add(d);
        }

        if (!remainingDates.Any())
        {
            return new JsonResult(new { success = false, error = "Không có ngày nào trong tuần này chưa tới ngày giao." });
        }

        var plan = await _aiMenuService.GenerateMenuAsync(
            UserId,
            remainingDates,
            string.IsNullOrWhiteSpace(request.WeeklyNotes) ? null : request.WeeklyNotes.Trim()
        );
        var orderedPlan = plan.OrderBy(p => p.day).ToList();

        for (var i = 0; i < remainingDates.Count && i < orderedPlan.Count; i++)
        {
            var date = remainingDates[i];
            var dayPlan = orderedPlan[i];

            if (dayPlan.meal_ids == null || dayPlan.meal_ids.Count == 0)
                continue;

            // Bữa 1 → Morning (1), Bữa 2 → Evening (3), Bữa 3 → Afternoon (2)
            var slotMap = new[] { 1, 3, 2 };
            var dto = new SaveDaySelectionsDto
            {
                Date = date,
                Selections = dayPlan
                    .meal_ids.Select((id, index) => new SlotSelection
                    {
                        MealId = id,
                        DeliverySlotId = index < slotMap.Length ? slotMap[index] : 1,
                    })
                    .ToList(),
            };

            await _menuService.SaveDaySelectionsAsync(UserId, sub.Id, dto);
        }

        return new JsonResult(
            new
            {
                success = true,
                plannedDays = remainingDates,
            }
        );
    }
}
