using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class DashboardDto
{
    public string UserFullName { get; set; } = "";
    public int UserCalorieTarget { get; set; } = 2000;
    public SubscriptionStatus? SubscriptionStatus { get; set; }
    public DateOnly? NextDeliveryDate { get; set; }
    public string? PlanName { get; set; }
    public int TodayCalories { get; set; }
    public decimal TodayProtein { get; set; }
    public decimal TodayCarbs { get; set; }
    public decimal TodayFat { get; set; }
    public List<MealListItemDto> FeaturedMeals { get; set; } = [];
    public int PendingFeedbackCount { get; set; }
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(Guid userId);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _ctx;
    private readonly IMealService _mealService;

    public DashboardService(AppDbContext ctx, IMealService mealService)
    {
        _ctx = ctx;
        _mealService = mealService;
    }

    public async Task<DashboardDto> GetDashboardAsync(Guid userId)
    {
        var user =
            await _ctx.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        // Active subscription + next delivery
        var sub = await _ctx
            .Subscriptions.Include(s => s.Plan)
            .Where(s =>
                s.UserId == userId && s.Status == BusinessObjects.Enums.SubscriptionStatus.Active
            )
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();

        DateOnly? nextDelivery = null;
        if (sub != null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var nextOrder = await _ctx
                .Orders.Where(o =>
                    o.SubscriptionId == sub.Id
                    && o.DeliveryDate >= today
                    && o.Status == OrderStatus.Planned
                )
                .OrderBy(o => o.DeliveryDate)
                .Select(o => (DateOnly?)o.DeliveryDate)
                .FirstOrDefaultAsync();
            nextDelivery = nextOrder;
        }

        // Today's nutrition from NutritionLogs
        var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayLogs = await _ctx
            .NutritionLogs.Include(l => l.Meal)
            .Where(l => l.UserId == userId && l.Date == todayDate)
            .ToListAsync();

        var featured = await _mealService.GetFeaturedAsync(8);

        // Pending feedback count: delivered items not yet rated
        var ratedOrderItemIds = await _ctx.MealRatings
            .Where(r => r.UserId == userId)
            .Select(r => r.OrderItemId)
            .ToListAsync();

        var pendingFeedbackCount = await _ctx.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Order!.UserId == userId
                && oi.Status == BusinessObjects.Enums.OrderItemStatus.Delivered
                && !ratedOrderItemIds.Contains(oi.Id))
            .CountAsync();

        return new DashboardDto
        {
            UserFullName = user.FullName,
            UserCalorieTarget = user.CaloriesInDay ?? 2000,
            SubscriptionStatus = sub?.Status,
            NextDeliveryDate = nextDelivery,
            PlanName = sub?.Plan?.Name,
            TodayCalories = todayLogs.Sum(l => (l.Meal?.Calories ?? 0) * l.Quantity),
            TodayProtein = todayLogs.Sum(l => (l.Meal?.Protein ?? 0) * l.Quantity),
            TodayCarbs = todayLogs.Sum(l => (l.Meal?.Carbs ?? 0) * l.Quantity),
            TodayFat = todayLogs.Sum(l => (l.Meal?.Fat ?? 0) * l.Quantity),
            FeaturedMeals = featured,
            PendingFeedbackCount = pendingFeedbackCount,
        };
    }
}
