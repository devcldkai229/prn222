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

public record TopSellingMeal(string Name, int Quantity);

public record LowRatedMeal(string Name, double AvgStars, int RatingCount);

public record DeliverySlotLoad(string SlotName, int Capacity, int Used);

public record ShipperPerfRow(string ShipperName, int OrdersDelivered);

public record DashboardAlertItem(string Severity, string Title, string Detail);

public record QuarterlyComparisonRow(string QuarterLabel, decimal Revenue, decimal? QuarterOverQuarterChangePercent);

public record YearlyComparisonRow(int Year, decimal Revenue, decimal? YearOverYearChangePercent);

public record StrategicMetricRow(
    string Metric,
    string Format,
    decimal CurrentQuarter,
    decimal PreviousQuarter,
    decimal? QuarterOverQuarterChangePercent,
    decimal CurrentYear,
    decimal PreviousYear,
    decimal? YearOverYearChangePercent
);

public class AdminDashboardViewModel
{
    public int SelectedDays { get; set; } = 30;
    public DateOnly RangeStartDate { get; set; }
    public DateOnly RangeEndDate { get; set; }
    public bool IsCustomRange { get; set; }

    public decimal TodaysRevenue { get; set; }
    public decimal YesterdaysRevenue { get; set; }
    public decimal RevenueInRange { get; set; }
    public int ActiveSubscriberCount { get; set; }
    public int NewUserCountInRange { get; set; }
    public int TotalOrdersInRange { get; set; }
    public int DeliveredOrdersInRange { get; set; }
    public int CancelledOrdersInRange { get; set; }
    public decimal OrderCompletionRate { get; set; }
    public decimal PaymentSuccessRate { get; set; }
    public int TomorrowOrderCount { get; set; }
    public int KitchenPrepMealCount { get; set; }
    public decimal CurrentQuarterRevenue { get; set; }
    public decimal PreviousQuarterRevenue { get; set; }
    public decimal QuarterOverQuarterChangePercent { get; set; }
    public decimal CurrentYearRevenue { get; set; }
    public decimal PreviousYearRevenue { get; set; }
    public decimal YearOverYearChangePercent { get; set; }

    public List<int> RevenueComparisonYears { get; set; } = new();
    public int SelectedQuarter1Year { get; set; }
    public int SelectedQuarter1 { get; set; }
    public int SelectedQuarter2Year { get; set; }
    public int SelectedQuarter2 { get; set; }
    public decimal SelectedQuarter1Revenue { get; set; }
    public decimal SelectedQuarter2Revenue { get; set; }
    public decimal SelectedQuarterRevenueDiffAmount { get; set; }
    public decimal? SelectedQuarterRevenueDiffPercent { get; set; }

    public int SelectedYear1 { get; set; }
    public int SelectedYear2 { get; set; }
    public decimal SelectedYear1Revenue { get; set; }
    public decimal SelectedYear2Revenue { get; set; }
    public decimal SelectedYearRevenueDiffAmount { get; set; }
    public decimal? SelectedYearRevenueDiffPercent { get; set; }

    public List<RevenueDataPoint> RevenueLastThirtyDays { get; set; } = new();
    public List<GoalSegment> UserGoalSegments { get; set; } = new();
    public List<TopDislikedMeal> TopDislikedMeals { get; set; } = new();
    public List<OrderStatusSegment> OrderStatusDistribution { get; set; } = new();
    public List<FailedPaymentRow> RecentFailedPayments { get; set; } = new();
    public List<NewSubDataPoint> NewSubscriptionsLast7Days { get; set; } = new();
    public List<PlanSegment> SubscriptionPlanDistribution { get; set; } = new();
    public List<TopSellingMeal> TopSellingMeals { get; set; } = new();
    public List<LowRatedMeal> LowRatedMeals { get; set; } = new();
    public List<DeliverySlotLoad> DeliverySlotLoadsTomorrow { get; set; } = new();
    public List<ShipperPerfRow> TopShippersInRange { get; set; } = new();
    public List<DashboardAlertItem> Alerts { get; set; } = new();
    public List<QuarterlyComparisonRow> QuarterlyRevenueCurrentYear { get; set; } = new();
    public List<YearlyComparisonRow> YearlyRevenueLastFiveYears { get; set; } = new();
    public List<StrategicMetricRow> StrategicMetrics { get; set; } = new();
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetDashboardAsync(
        int days = 30,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        int? quarter1Year = null,
        int? quarter1 = null,
        int? quarter2Year = null,
        int? quarter2 = null,
        int? year1 = null,
        int? year2 = null
    );
}

// ── Implementation ────────────────────────────────────────────────────────────

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _ctx;

    public AdminDashboardService(AppDbContext ctx) => _ctx = ctx;

    public async Task<AdminDashboardViewModel> GetDashboardAsync(
        int days = 30,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        int? quarter1Year = null,
        int? quarter1 = null,
        int? quarter2Year = null,
        int? quarter2 = null,
        int? year1 = null,
        int? year2 = null
    )
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tomorrow = today.AddDays(1);
        var utcNow = DateTime.UtcNow;
        var hasCustomRange = fromDate.HasValue && toDate.HasValue;
        var selectedFromDate = hasCustomRange ? fromDate!.Value : today.AddDays(-(Math.Max(days, 1) - 1));
        var selectedToDate = hasCustomRange ? toDate!.Value : today;

        if (selectedFromDate > selectedToDate)
        {
            (selectedFromDate, selectedToDate) = (selectedToDate, selectedFromDate);
        }

        days = Math.Max(1, selectedToDate.DayNumber - selectedFromDate.DayNumber + 1);

        var fromDateOnly = selectedFromDate;
        var toDateOnly = selectedToDate;
        var fromDateTime = DateTime.SpecifyKind(fromDateOnly.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toDateTimeExclusive = DateTime.SpecifyKind(toDateOnly.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var currentYear = utcNow.Year;
        var currentQuarter = ((utcNow.Month - 1) / 3) + 1;

        // EF Core DbContext is NOT thread-safe — run queries sequentially
        var todaysRevenue = await _ctx.Payments
            .Where(p => p.Status == "Paid" && p.PaidAt.HasValue && p.PaidAt.Value.Date == DateTime.UtcNow.Date)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var yesterdaysRevenue = await _ctx.Payments
            .Where(p => p.Status == "Paid" && p.PaidAt.HasValue && p.PaidAt.Value.Date == DateTime.UtcNow.Date.AddDays(-1))
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var revenueInRange = await _ctx.Payments
            .Where(p => p.Status == "Paid"
                && p.PaidAt.HasValue
                && p.PaidAt.Value >= fromDateTime
                && p.PaidAt.Value < toDateTimeExclusive)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var activeSubscriberCount = await _ctx.Subscriptions
            .CountAsync(s => s.Status == SubscriptionStatus.Active);

        var newUserCountInRange = await _ctx.Users
            .CountAsync(u => u.CreatedAtUtc >= fromDateTime && u.CreatedAtUtc < toDateTimeExclusive);

        var totalOrdersInRange = await _ctx.Orders
            .CountAsync(o => o.DeliveryDate >= fromDateOnly && o.DeliveryDate <= toDateOnly);

        var deliveredOrdersInRange = await _ctx.Orders
            .CountAsync(o => o.DeliveryDate >= fromDateOnly
                && o.DeliveryDate <= toDateOnly
                && (o.Status == OrderStatus.Delivered
                    || o.Status == OrderStatus.ConfirmedByUser
                    || o.Status == OrderStatus.Completed));

        var cancelledOrdersInRange = await _ctx.Orders
            .CountAsync(o => o.DeliveryDate >= fromDateOnly
                && o.DeliveryDate <= toDateOnly
                && o.Status == OrderStatus.Cancelled);

        var paymentTotalInRange = await _ctx.Payments
            .CountAsync(p => p.CreatedAt >= fromDateTime
                && p.CreatedAt < toDateTimeExclusive
                && (p.Status == "Paid" || p.Status == "Failed" || p.Status == "Cancelled" || p.Status == "Expired"));

        var paymentPaidInRange = await _ctx.Payments
            .CountAsync(p => p.CreatedAt >= fromDateTime && p.CreatedAt < toDateTimeExclusive && p.Status == "Paid");

        var tomorrowOrderCount = await _ctx.Orders
            .CountAsync(o => o.DeliveryDate == tomorrow);

        var kitchenPrepMealCount = await _ctx.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Order!.DeliveryDate == tomorrow)
            .SumAsync(oi => (int?)oi.Quantity) ?? 0;

        var revenueChartRaw = await _ctx.Payments
            .Where(p => p.Status == "Paid"
                && p.PaidAt.HasValue
                && p.PaidAt.Value >= fromDateTime
                && p.PaidAt.Value < toDateTimeExclusive)
            .GroupBy(p => p.PaidAt!.Value.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(p => p.Amount) })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var revenueMap = revenueChartRaw.ToDictionary(x => DateOnly.FromDateTime(x.Date), x => x.Total);
        var revenueSeries = new List<RevenueDataPoint>();

        if (days >= 90)
        {
            // Group 90-day view into 3-day buckets so chart remains readable.
            for (var cursor = fromDateOnly; cursor <= today; cursor = cursor.AddDays(3))
            {
                var end = cursor.AddDays(2);
                if (end > today)
                {
                    end = today;
                }

                decimal bucketTotal = 0m;
                for (var d = cursor; d <= end; d = d.AddDays(1))
                {
                    if (revenueMap.TryGetValue(d, out var amount))
                    {
                        bucketTotal += amount;
                    }
                }

                revenueSeries.Add(new RevenueDataPoint($"{cursor:dd/MM}-{end:dd/MM}", bucketTotal));
            }
        }
        else
        {
            for (var d = fromDateOnly; d <= today; d = d.AddDays(1))
            {
                revenueSeries.Add(new RevenueDataPoint(
                    d.ToString("dd/MM"),
                    revenueMap.TryGetValue(d, out var amount) ? amount : 0m
                ));
            }
        }

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
            .Where(o => o.DeliveryDate >= fromDateOnly && o.DeliveryDate <= toDateOnly)
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

        var topSellingMeals = await _ctx.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Meal)
            .Where(oi => oi.Order != null
                && oi.Order.DeliveryDate >= fromDateOnly
                && oi.Order.DeliveryDate <= toDateOnly
                && oi.Meal != null)
            .GroupBy(oi => oi.Meal!.Name)
            .Select(g => new { Name = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Quantity)
            .Take(6)
            .ToListAsync();

        var lowRatedMeals = await _ctx.MealRatings
            .Include(r => r.Meal)
            .Where(r => r.CreatedAt >= fromDateTime
                && r.CreatedAt < toDateTimeExclusive
                && r.Meal != null)
            .GroupBy(r => r.Meal!.Name)
            .Select(g => new
            {
                Name = g.Key,
                AvgStars = g.Average(x => (double)x.Stars),
                RatingCount = g.Count()
            })
            .Where(x => x.RatingCount >= 2)
            .OrderBy(x => x.AvgStars)
            .ThenByDescending(x => x.RatingCount)
            .Take(6)
            .ToListAsync();

        var slotLoadsTomorrow = await _ctx.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.DeliverySlot)
            .Where(oi => oi.Order != null
                && oi.Order.DeliveryDate == tomorrow
                && oi.DeliverySlot != null)
            .GroupBy(oi => new { oi.DeliverySlot!.Name, oi.DeliverySlot.Capacity })
            .Select(g => new
            {
                SlotName = g.Key.Name,
                Capacity = g.Key.Capacity,
                Used = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Used)
            .ToListAsync();

        var topShippersInRange = await _ctx.Orders
            .Include(o => o.Shipper)
            .Where(o => o.DeliveryDate >= fromDateOnly
                && o.DeliveryDate <= toDateOnly
                && o.ShipperId.HasValue
                && (o.Status == OrderStatus.Delivered
                    || o.Status == OrderStatus.ConfirmedByUser
                    || o.Status == OrderStatus.Completed))
            .GroupBy(o => o.Shipper!.FullName)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        var quarterlyRevenueRaw = await _ctx.Payments
            .Where(p => p.Status == "Paid"
                && p.PaidAt.HasValue
                && p.PaidAt.Value.Year >= currentYear - 5)
            .GroupBy(p => new
            {
                Year = p.PaidAt!.Value.Year,
                Quarter = ((p.PaidAt.Value.Month - 1) / 3) + 1
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Quarter,
                Revenue = g.Sum(x => x.Amount)
            })
            .ToListAsync();

        var minTrackDate = DateOnly.FromDateTime(new DateTime(currentYear - 5, 1, 1));

        var orderQuarterlyRaw = await _ctx.Orders
            .Where(o => o.DeliveryDate >= minTrackDate)
            .GroupBy(o => new
            {
                Year = o.DeliveryDate.Year,
                Quarter = ((o.DeliveryDate.Month - 1) / 3) + 1
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Quarter,
                Total = g.Count(),
                Completed = g.Count(x =>
                    x.Status == OrderStatus.Delivered
                    || x.Status == OrderStatus.ConfirmedByUser
                    || x.Status == OrderStatus.Completed),
                Cancelled = g.Count(x => x.Status == OrderStatus.Cancelled),
            })
            .ToListAsync();

        var orderYearlyRaw = await _ctx.Orders
            .Where(o => o.DeliveryDate >= minTrackDate)
            .GroupBy(o => o.DeliveryDate.Year)
            .Select(g => new
            {
                Year = g.Key,
                Total = g.Count(),
                Completed = g.Count(x =>
                    x.Status == OrderStatus.Delivered
                    || x.Status == OrderStatus.ConfirmedByUser
                    || x.Status == OrderStatus.Completed),
                Cancelled = g.Count(x => x.Status == OrderStatus.Cancelled),
            })
            .ToListAsync();

        var paymentQuarterlyRaw = await _ctx.Payments
            .Where(p => p.CreatedAt.Year >= currentYear - 5
                && (p.Status == "Paid" || p.Status == "Failed" || p.Status == "Cancelled" || p.Status == "Expired"))
            .GroupBy(p => new
            {
                Year = p.CreatedAt.Year,
                Quarter = ((p.CreatedAt.Month - 1) / 3) + 1
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Quarter,
                Total = g.Count(),
                Paid = g.Count(x => x.Status == "Paid"),
            })
            .ToListAsync();

        var paymentYearlyRaw = await _ctx.Payments
            .Where(p => p.CreatedAt.Year >= currentYear - 5
                && (p.Status == "Paid" || p.Status == "Failed" || p.Status == "Cancelled" || p.Status == "Expired"))
            .GroupBy(p => p.CreatedAt.Year)
            .Select(g => new
            {
                Year = g.Key,
                Total = g.Count(),
                Paid = g.Count(x => x.Status == "Paid"),
            })
            .ToListAsync();

        var newUserQuarterlyRaw = await _ctx.Users
            .Where(u => u.CreatedAtUtc.Year >= currentYear - 5)
            .GroupBy(u => new
            {
                Year = u.CreatedAtUtc.Year,
                Quarter = ((u.CreatedAtUtc.Month - 1) / 3) + 1
            })
            .Select(g => new { g.Key.Year, g.Key.Quarter, Count = g.Count() })
            .ToListAsync();

        var newUserYearlyRaw = await _ctx.Users
            .Where(u => u.CreatedAtUtc.Year >= currentYear - 5)
            .GroupBy(u => u.CreatedAtUtc.Year)
            .Select(g => new { Year = g.Key, Count = g.Count() })
            .ToListAsync();

        var newSubQuarterlyRaw = await _ctx.Subscriptions
            .Where(s => s.CreatedAt.Year >= currentYear - 5)
            .GroupBy(s => new
            {
                Year = s.CreatedAt.Year,
                Quarter = ((s.CreatedAt.Month - 1) / 3) + 1
            })
            .Select(g => new { g.Key.Year, g.Key.Quarter, Count = g.Count() })
            .ToListAsync();

        var newSubYearlyRaw = await _ctx.Subscriptions
            .Where(s => s.CreatedAt.Year >= currentYear - 5)
            .GroupBy(s => s.CreatedAt.Year)
            .Select(g => new { Year = g.Key, Count = g.Count() })
            .ToListAsync();

        var yearlyRevenueRaw = await _ctx.Payments
            .Where(p => p.Status == "Paid"
                && p.PaidAt.HasValue
                && p.PaidAt.Value.Year >= currentYear - 5)
            .GroupBy(p => p.PaidAt!.Value.Year)
            .Select(g => new
            {
                Year = g.Key,
                Revenue = g.Sum(x => x.Amount)
            })
            .ToListAsync();

        var quarterRevenueMap = quarterlyRevenueRaw
            .ToDictionary(x => (x.Year, x.Quarter), x => x.Revenue);
        var yearRevenueMap = yearlyRevenueRaw
            .ToDictionary(x => x.Year, x => x.Revenue);

        var orderQuarterMap = orderQuarterlyRaw
            .ToDictionary(x => (x.Year, x.Quarter), x => (x.Total, x.Completed, x.Cancelled));
        var orderYearMap = orderYearlyRaw
            .ToDictionary(x => x.Year, x => (x.Total, x.Completed, x.Cancelled));

        var paymentQuarterMap = paymentQuarterlyRaw
            .ToDictionary(x => (x.Year, x.Quarter), x => (x.Total, x.Paid));
        var paymentYearMap = paymentYearlyRaw
            .ToDictionary(x => x.Year, x => (x.Total, x.Paid));

        var newUserQuarterMap = newUserQuarterlyRaw
            .ToDictionary(x => (x.Year, x.Quarter), x => x.Count);
        var newUserYearMap = newUserYearlyRaw
            .ToDictionary(x => x.Year, x => x.Count);

        var newSubQuarterMap = newSubQuarterlyRaw
            .ToDictionary(x => (x.Year, x.Quarter), x => x.Count);
        var newSubYearMap = newSubYearlyRaw
            .ToDictionary(x => x.Year, x => x.Count);

        decimal GetQuarterRevenue(int year, int quarter)
            => quarterRevenueMap.TryGetValue((year, quarter), out var value) ? value : 0m;

        decimal GetYearRevenue(int year)
            => yearRevenueMap.TryGetValue(year, out var value) ? value : 0m;

        (int Total, int Completed, int Cancelled) GetOrderQuarter(int year, int quarter)
            => orderQuarterMap.TryGetValue((year, quarter), out var value) ? value : (0, 0, 0);

        (int Total, int Completed, int Cancelled) GetOrderYear(int year)
            => orderYearMap.TryGetValue(year, out var value) ? value : (0, 0, 0);

        (int Total, int Paid) GetPaymentQuarter(int year, int quarter)
            => paymentQuarterMap.TryGetValue((year, quarter), out var value) ? value : (0, 0);

        (int Total, int Paid) GetPaymentYear(int year)
            => paymentYearMap.TryGetValue(year, out var value) ? value : (0, 0);

        int GetNewUsersQuarter(int year, int quarter)
            => newUserQuarterMap.TryGetValue((year, quarter), out var value) ? value : 0;

        int GetNewUsersYear(int year)
            => newUserYearMap.TryGetValue(year, out var value) ? value : 0;

        int GetNewSubsQuarter(int year, int quarter)
            => newSubQuarterMap.TryGetValue((year, quarter), out var value) ? value : 0;

        int GetNewSubsYear(int year)
            => newSubYearMap.TryGetValue(year, out var value) ? value : 0;

        decimal CompletionRate(int total, int completed)
            => total > 0 ? decimal.Round((decimal)completed * 100m / total, 1) : 0m;

        decimal CancelRate(int total, int cancelled)
            => total > 0 ? decimal.Round((decimal)cancelled * 100m / total, 1) : 0m;

        decimal PaymentSuccessRate(int total, int paid)
            => total > 0 ? decimal.Round((decimal)paid * 100m / total, 1) : 0m;

        decimal? ChangePercent(decimal current, decimal previous)
            => previous > 0m ? decimal.Round((current - previous) * 100m / previous, 1) : null;

        (int PrevYear, int PrevQuarter) GetPreviousQuarter(int year, int quarter)
            => quarter == 1 ? (year - 1, 4) : (year, quarter - 1);

        var quarterlyRevenueCurrentYear = new List<QuarterlyComparisonRow>();
        for (var q = 1; q <= 4; q++)
        {
            var revenue = GetQuarterRevenue(currentYear, q);
            var (prevQYear, prevQ) = GetPreviousQuarter(currentYear, q);
            var prevRevenue = GetQuarterRevenue(prevQYear, prevQ);
            decimal? qoq = prevRevenue > 0m
                ? decimal.Round((revenue - prevRevenue) * 100m / prevRevenue, 1)
                : (decimal?)null;

            quarterlyRevenueCurrentYear.Add(new QuarterlyComparisonRow(
                $"Q{q}/{currentYear}",
                revenue,
                qoq
            ));
        }

        var yearlyRevenueLastFiveYears = new List<YearlyComparisonRow>();
        for (var year = currentYear - 4; year <= currentYear; year++)
        {
            var revenue = GetYearRevenue(year);
            var prevYearRevenue = GetYearRevenue(year - 1);
            decimal? yoy = prevYearRevenue > 0m
                ? decimal.Round((revenue - prevYearRevenue) * 100m / prevYearRevenue, 1)
                : (decimal?)null;

            yearlyRevenueLastFiveYears.Add(new YearlyComparisonRow(
                year,
                revenue,
                yoy
            ));
        }

        var (previousQuarterYear, previousQuarter) = GetPreviousQuarter(currentYear, currentQuarter);
        var currentQuarterRevenue = GetQuarterRevenue(currentYear, currentQuarter);
        var previousQuarterRevenue = GetQuarterRevenue(previousQuarterYear, previousQuarter);
        var quarterOverQuarterChangePercent = previousQuarterRevenue > 0m
            ? decimal.Round((currentQuarterRevenue - previousQuarterRevenue) * 100m / previousQuarterRevenue, 1)
            : 0m;

        var currentYearRevenue = GetYearRevenue(currentYear);
        var previousYearRevenue = GetYearRevenue(currentYear - 1);
        var yearOverYearChangePercent = previousYearRevenue > 0m
            ? decimal.Round((currentYearRevenue - previousYearRevenue) * 100m / previousYearRevenue, 1)
            : 0m;

        var revenueComparisonYears = quarterlyRevenueRaw
            .Select(x => x.Year)
            .Concat(yearlyRevenueRaw.Select(x => x.Year))
            .Concat(Enumerable.Range(currentYear - 5, 6))
            .Where(y => y <= currentYear)
            .Distinct()
            .OrderByDescending(y => y)
            .ToList();

        if (revenueComparisonYears.Count == 0)
        {
            revenueComparisonYears.Add(currentYear);
        }

        var selectedQuarter1Year = quarter1Year.HasValue && revenueComparisonYears.Contains(quarter1Year.Value)
            ? quarter1Year.Value
            : currentYear;
        var selectedQuarter2Year = quarter2Year.HasValue && revenueComparisonYears.Contains(quarter2Year.Value)
            ? quarter2Year.Value
            : previousQuarterYear;

        selectedQuarter2Year = revenueComparisonYears.Contains(selectedQuarter2Year)
            ? selectedQuarter2Year
            : revenueComparisonYears.First();

        var selectedQuarter1 = quarter1 is >= 1 and <= 4 ? quarter1.Value : currentQuarter;
        var selectedQuarter2 = quarter2 is >= 1 and <= 4 ? quarter2.Value : previousQuarter;

        var selectedQuarter1Revenue = GetQuarterRevenue(selectedQuarter1Year, selectedQuarter1);
        var selectedQuarter2Revenue = GetQuarterRevenue(selectedQuarter2Year, selectedQuarter2);
        var selectedQuarterRevenueDiffAmount = selectedQuarter1Revenue - selectedQuarter2Revenue;
        var selectedQuarterRevenueDiffPercent = selectedQuarter2Revenue > 0m
            ? decimal.Round(selectedQuarterRevenueDiffAmount * 100m / selectedQuarter2Revenue, 1)
            : (decimal?)null;

        var selectedYear1 = year1.HasValue && revenueComparisonYears.Contains(year1.Value)
            ? year1.Value
            : currentYear;
        var selectedYear2 = year2.HasValue && revenueComparisonYears.Contains(year2.Value)
            ? year2.Value
            : currentYear - 1;

        selectedYear2 = revenueComparisonYears.Contains(selectedYear2)
            ? selectedYear2
            : revenueComparisonYears.First();

        var selectedYear1Revenue = GetYearRevenue(selectedYear1);
        var selectedYear2Revenue = GetYearRevenue(selectedYear2);
        var selectedYearRevenueDiffAmount = selectedYear1Revenue - selectedYear2Revenue;
        var selectedYearRevenueDiffPercent = selectedYear2Revenue > 0m
            ? decimal.Round(selectedYearRevenueDiffAmount * 100m / selectedYear2Revenue, 1)
            : (decimal?)null;

        var currentQuarterOrders = GetOrderQuarter(currentYear, currentQuarter);
        var previousQuarterOrders = GetOrderQuarter(previousQuarterYear, previousQuarter);
        var currentYearOrders = GetOrderYear(currentYear);
        var previousYearOrders = GetOrderYear(currentYear - 1);

        var currentQuarterPayments = GetPaymentQuarter(currentYear, currentQuarter);
        var previousQuarterPayments = GetPaymentQuarter(previousQuarterYear, previousQuarter);
        var currentYearPayments = GetPaymentYear(currentYear);
        var previousYearPayments = GetPaymentYear(currentYear - 1);

        var strategicMetrics = new List<StrategicMetricRow>
        {
            new(
                "Doanh thu",
                "currency",
                currentQuarterRevenue,
                previousQuarterRevenue,
                ChangePercent(currentQuarterRevenue, previousQuarterRevenue),
                currentYearRevenue,
                previousYearRevenue,
                ChangePercent(currentYearRevenue, previousYearRevenue)
            ),
            new(
                "Tổng đơn hàng",
                "number",
                currentQuarterOrders.Total,
                previousQuarterOrders.Total,
                ChangePercent(currentQuarterOrders.Total, previousQuarterOrders.Total),
                currentYearOrders.Total,
                previousYearOrders.Total,
                ChangePercent(currentYearOrders.Total, previousYearOrders.Total)
            ),
            new(
                "Tỉ lệ hoàn tất đơn",
                "percent",
                CompletionRate(currentQuarterOrders.Total, currentQuarterOrders.Completed),
                CompletionRate(previousQuarterOrders.Total, previousQuarterOrders.Completed),
                ChangePercent(
                    CompletionRate(currentQuarterOrders.Total, currentQuarterOrders.Completed),
                    CompletionRate(previousQuarterOrders.Total, previousQuarterOrders.Completed)
                ),
                CompletionRate(currentYearOrders.Total, currentYearOrders.Completed),
                CompletionRate(previousYearOrders.Total, previousYearOrders.Completed),
                ChangePercent(
                    CompletionRate(currentYearOrders.Total, currentYearOrders.Completed),
                    CompletionRate(previousYearOrders.Total, previousYearOrders.Completed)
                )
            ),
            new(
                "Tỉ lệ hủy đơn",
                "percent",
                CancelRate(currentQuarterOrders.Total, currentQuarterOrders.Cancelled),
                CancelRate(previousQuarterOrders.Total, previousQuarterOrders.Cancelled),
                ChangePercent(
                    CancelRate(currentQuarterOrders.Total, currentQuarterOrders.Cancelled),
                    CancelRate(previousQuarterOrders.Total, previousQuarterOrders.Cancelled)
                ),
                CancelRate(currentYearOrders.Total, currentYearOrders.Cancelled),
                CancelRate(previousYearOrders.Total, previousYearOrders.Cancelled),
                ChangePercent(
                    CancelRate(currentYearOrders.Total, currentYearOrders.Cancelled),
                    CancelRate(previousYearOrders.Total, previousYearOrders.Cancelled)
                )
            ),
            new(
                "Tỉ lệ thanh toán thành công",
                "percent",
                PaymentSuccessRate(currentQuarterPayments.Total, currentQuarterPayments.Paid),
                PaymentSuccessRate(previousQuarterPayments.Total, previousQuarterPayments.Paid),
                ChangePercent(
                    PaymentSuccessRate(currentQuarterPayments.Total, currentQuarterPayments.Paid),
                    PaymentSuccessRate(previousQuarterPayments.Total, previousQuarterPayments.Paid)
                ),
                PaymentSuccessRate(currentYearPayments.Total, currentYearPayments.Paid),
                PaymentSuccessRate(previousYearPayments.Total, previousYearPayments.Paid),
                ChangePercent(
                    PaymentSuccessRate(currentYearPayments.Total, currentYearPayments.Paid),
                    PaymentSuccessRate(previousYearPayments.Total, previousYearPayments.Paid)
                )
            ),
            new(
                "Người dùng mới",
                "number",
                GetNewUsersQuarter(currentYear, currentQuarter),
                GetNewUsersQuarter(previousQuarterYear, previousQuarter),
                ChangePercent(
                    GetNewUsersQuarter(currentYear, currentQuarter),
                    GetNewUsersQuarter(previousQuarterYear, previousQuarter)
                ),
                GetNewUsersYear(currentYear),
                GetNewUsersYear(currentYear - 1),
                ChangePercent(
                    GetNewUsersYear(currentYear),
                    GetNewUsersYear(currentYear - 1)
                )
            ),
            new(
                "Đăng ký mới",
                "number",
                GetNewSubsQuarter(currentYear, currentQuarter),
                GetNewSubsQuarter(previousQuarterYear, previousQuarter),
                ChangePercent(
                    GetNewSubsQuarter(currentYear, currentQuarter),
                    GetNewSubsQuarter(previousQuarterYear, previousQuarter)
                ),
                GetNewSubsYear(currentYear),
                GetNewSubsYear(currentYear - 1),
                ChangePercent(
                    GetNewSubsYear(currentYear),
                    GetNewSubsYear(currentYear - 1)
                )
            ),
        };

        var alerts = new List<DashboardAlertItem>();
        if (cancelledOrdersInRange > 0 && totalOrdersInRange > 0)
        {
            var cancelRate = (decimal)cancelledOrdersInRange * 100m / totalOrdersInRange;
            if (cancelRate >= 12m)
            {
                alerts.Add(new DashboardAlertItem(
                    "critical",
                    "Tỷ lệ hủy đơn cao",
                    $"Tỷ lệ hủy {cancelRate:0.0}% trong {days} ngày gần đây."
                ));
            }
        }

        if (paymentTotalInRange > 0)
        {
            var paySuccessRate = (decimal)paymentPaidInRange * 100m / paymentTotalInRange;
            if (paySuccessRate < 85m)
            {
                alerts.Add(new DashboardAlertItem(
                    "warning",
                    "Tỷ lệ thanh toán thành công giảm",
                    $"Chỉ đạt {paySuccessRate:0.0}% trong {days} ngày qua."
                ));
            }
        }

        foreach (var slot in slotLoadsTomorrow)
        {
            if (slot.Capacity <= 0)
            {
                continue;
            }

            var ratio = (decimal)slot.Used / slot.Capacity;
            if (ratio >= 0.85m)
            {
                alerts.Add(new DashboardAlertItem(
                    ratio >= 1m ? "critical" : "warning",
                    "Slot giao hàng sắp quá tải",
                    $"{slot.SlotName}: {slot.Used}/{slot.Capacity} phần ăn ({ratio * 100m:0.#}%)."
                ));
            }
        }

        if (alerts.Count == 0)
        {
            alerts.Add(new DashboardAlertItem("info", "Hệ thống ổn định", "Chưa có cảnh báo nghiêm trọng trong giai đoạn này."));
        }

        return new AdminDashboardViewModel
        {
            SelectedDays = days,
            RangeStartDate = fromDateOnly,
            RangeEndDate = toDateOnly,
            IsCustomRange = hasCustomRange,
            TodaysRevenue = todaysRevenue,
            YesterdaysRevenue = yesterdaysRevenue,
            RevenueInRange = revenueInRange,
            ActiveSubscriberCount = activeSubscriberCount,
            NewUserCountInRange = newUserCountInRange,
            TotalOrdersInRange = totalOrdersInRange,
            DeliveredOrdersInRange = deliveredOrdersInRange,
            CancelledOrdersInRange = cancelledOrdersInRange,
            OrderCompletionRate = totalOrdersInRange > 0
                ? decimal.Round((decimal)deliveredOrdersInRange * 100m / totalOrdersInRange, 1)
                : 0m,
            PaymentSuccessRate = paymentTotalInRange > 0
                ? decimal.Round((decimal)paymentPaidInRange * 100m / paymentTotalInRange, 1)
                : 0m,
            TomorrowOrderCount = tomorrowOrderCount,
            KitchenPrepMealCount = kitchenPrepMealCount,
            CurrentQuarterRevenue = currentQuarterRevenue,
            PreviousQuarterRevenue = previousQuarterRevenue,
            QuarterOverQuarterChangePercent = quarterOverQuarterChangePercent,
            CurrentYearRevenue = currentYearRevenue,
            PreviousYearRevenue = previousYearRevenue,
            YearOverYearChangePercent = yearOverYearChangePercent,
            RevenueComparisonYears = revenueComparisonYears,
            SelectedQuarter1Year = selectedQuarter1Year,
            SelectedQuarter1 = selectedQuarter1,
            SelectedQuarter2Year = selectedQuarter2Year,
            SelectedQuarter2 = selectedQuarter2,
            SelectedQuarter1Revenue = selectedQuarter1Revenue,
            SelectedQuarter2Revenue = selectedQuarter2Revenue,
            SelectedQuarterRevenueDiffAmount = selectedQuarterRevenueDiffAmount,
            SelectedQuarterRevenueDiffPercent = selectedQuarterRevenueDiffPercent,
            SelectedYear1 = selectedYear1,
            SelectedYear2 = selectedYear2,
            SelectedYear1Revenue = selectedYear1Revenue,
            SelectedYear2Revenue = selectedYear2Revenue,
            SelectedYearRevenueDiffAmount = selectedYearRevenueDiffAmount,
            SelectedYearRevenueDiffPercent = selectedYearRevenueDiffPercent,

            RevenueLastThirtyDays = revenueSeries,

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

            TopSellingMeals = topSellingMeals
                .Select(x => new TopSellingMeal(x.Name, x.Quantity))
                .ToList(),

            LowRatedMeals = lowRatedMeals
                .Select(x => new LowRatedMeal(x.Name, x.AvgStars, x.RatingCount))
                .ToList(),

            DeliverySlotLoadsTomorrow = slotLoadsTomorrow
                .Select(x => new DeliverySlotLoad(x.SlotName, x.Capacity, x.Used))
                .ToList(),

            TopShippersInRange = topShippersInRange
                .Select(x => new ShipperPerfRow(x.Name, x.Count))
                .ToList(),

            Alerts = alerts,

            QuarterlyRevenueCurrentYear = quarterlyRevenueCurrentYear,

            YearlyRevenueLastFiveYears = yearlyRevenueLastFiveYears,

            StrategicMetrics = strategicMetrics,
        };
    }
}
