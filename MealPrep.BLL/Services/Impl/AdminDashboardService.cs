using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record KpiCard(
    string Title,
    string Value,
    string Icon,
    string Color,
    string? Trend = null,
    string? TrendLabel = null
);

public record RevenueDataPoint(string Label, decimal Amount);

public record OrderStatusSegment(string Status, int Count);

public record GoalSegment(string Goal, int Count);

public record TopDislikedMeal(string Name, int DislikedCount);

public record FailedPaymentRow(
    int PaymentId,
    string CustomerName,
    decimal Amount,
    string Method,
    DateTime CreatedAt
);

public record NewSubDataPoint(string Label, int Count);

public record PlanSegment(string PlanName, int Count);

public class AdminDashboardViewModel
{
    public decimal TodaysRevenue { get; set; }
    public decimal YesterdaysRevenue { get; set; }
    public int ActiveSubscriberCount { get; set; }
    public int TomorrowOrderCount { get; set; }
    public int KitchenPrepMealCount { get; set; }

    public List<RevenueDataPoint> RevenueLastThirtyDays { get; set; } = new();
    public List<GoalSegment> UserGoalSegments { get; set; } = new();
    public List<TopDislikedMeal> TopDislikedMeals { get; set; } = new();
    public List<OrderStatusSegment> OrderStatusDistribution { get; set; } = new();
    public List<FailedPaymentRow> RecentFailedPayments { get; set; } = new();
    public List<NewSubDataPoint> NewSubscriptionsLast7Days { get; set; } = new();
    public List<PlanSegment> SubscriptionPlanDistribution { get; set; } = new();
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetDashboardAsync();
}

// ── Implementation ────────────────────────────────────────────────────────────

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _ctx;

    public AdminDashboardService(AppDbContext ctx) => _ctx = ctx;

    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tomorrow = today.AddDays(1);
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        // EF Core DbContext is NOT thread-safe — run queries sequentially
        var todaysRevenue = await _ctx.Payments
            .Where(p => p.Status == "Paid" && p.PaidAt.HasValue && p.PaidAt.Value.Date == DateTime.UtcNow.Date)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var yesterdaysRevenue = await _ctx.Payments
            .Where(p => p.Status == "Paid" && p.PaidAt.HasValue && p.PaidAt.Value.Date == DateTime.UtcNow.Date.AddDays(-1))
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var activeSubscriberCount = await _ctx.Subscriptions
            .CountAsync(s => s.Status == SubscriptionStatus.Active);

        var tomorrowOrderCount = await _ctx.Orders
            .CountAsync(o => o.DeliveryDate == tomorrow);

        var kitchenPrepMealCount = await _ctx.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Order!.DeliveryDate == tomorrow)
            .SumAsync(oi => (int?)oi.Quantity) ?? 0;

        var revenueChart = await _ctx.Payments
            .Where(p => p.Status == "Paid" && p.PaidAt >= thirtyDaysAgo)
            .GroupBy(p => p.PaidAt!.Value.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(p => p.Amount) })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var goalSegments = await _ctx.Users
            .GroupBy(u => u.Goal)
            .Select(g => new { Goal = g.Key, Count = g.Count() })
            .ToListAsync();

        var topDisliked = await _ctx.Users
            .SelectMany(u => u.DislikedMeals!)
            .GroupBy(m => m.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        var orderStatus = await _ctx.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var failedPayments = await _ctx.Payments
            .Include(p => p.User)
            .Where(p => p.Status == "Failed")
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync();

        // New subscriptions in last 7 days (grouped by day)
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var newSubs7d = await _ctx.Subscriptions
            .Where(s => s.CreatedAt >= sevenDaysAgo)
            .GroupBy(s => s.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        // Active subscriptions by plan
        var planDist = await _ctx.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.Status == SubscriptionStatus.Active && s.Plan != null)
            .GroupBy(s => s.Plan!.Name)
            .Select(g => new { PlanName = g.Key, Count = g.Count() })
            .ToListAsync();

        return new AdminDashboardViewModel
        {
            TodaysRevenue = todaysRevenue,
            YesterdaysRevenue = yesterdaysRevenue,
            ActiveSubscriberCount = activeSubscriberCount,
            TomorrowOrderCount = tomorrowOrderCount,
            KitchenPrepMealCount = kitchenPrepMealCount,

            RevenueLastThirtyDays = revenueChart
                .Select(x => new RevenueDataPoint(x.Date.ToString("MMM d"), x.Total))
                .ToList(),

            UserGoalSegments = goalSegments
                .Select(x => new GoalSegment(x.Goal.ToString(), x.Count))
                .ToList(),

            TopDislikedMeals = topDisliked
                .Select(x => new TopDislikedMeal(x.Name, x.Count))
                .ToList(),

            OrderStatusDistribution = orderStatus
                .Select(x => new OrderStatusSegment(x.Status.ToString(), x.Count))
                .ToList(),

            RecentFailedPayments = failedPayments
                .Select(p => new FailedPaymentRow(
                    p.Id,
                    p.User?.FullName ?? "Unknown",
                    p.Amount,
                    p.Method,
                    p.CreatedAt
                ))
                .ToList(),

            NewSubscriptionsLast7Days = newSubs7d
                .Select(x => new NewSubDataPoint(x.Date.ToString("dd/MM"), x.Count))
                .ToList(),

            SubscriptionPlanDistribution = planDist
                .Select(x => new PlanSegment(x.PlanName, x.Count))
                .ToList(),
        };
    }
}
