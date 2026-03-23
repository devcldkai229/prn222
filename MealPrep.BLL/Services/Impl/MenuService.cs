using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.BLL.Services.Interfaces;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class MealSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string[] ImageUrls { get; set; } = [];
    public decimal Calories { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
    public string[] Ingredients { get; set; } = [];
    public bool HasAllergenWarning { get; set; }
    public string[] MatchedAllergens { get; set; } = [];
}

public class SaveDaySelectionsDto
{
    public DateOnly Date { get; set; }
    public List<SlotSelection> Selections { get; set; } = [];
}

public class SlotSelection
{
    public int MealId { get; set; }
    public int DeliverySlotId { get; set; }
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IMenuService
{
    Task<List<MealSummaryDto>> GetActiveMealsAsync(string[] userAllergies);
    Task SaveDaySelectionsAsync(Guid userId, int subscriptionId, SaveDaySelectionsDto dto);
    Task<List<(DateOnly Date, List<MealSummaryDto> Meals)>> GetWeekSelectionsAsync(
        Guid userId,
        DateOnly weekStart
    );
    Task<List<(DateOnly Date, List<MealSummaryDto> Meals)>> GetSelectionsForDateRangeAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        int? subscriptionId = null
    );
    Task<List<DateOnly>> GetRemainingDatesInWeekAsync(
        Guid userId,
        int subscriptionId,
        DateOnly weekStart,
        DateOnly fromDate
    );
}

// ── Implementation ────────────────────────────────────────────────────────────

public class MenuService : IMenuService
{
    private readonly AppDbContext _ctx;
    private readonly IS3Service _s3;

    public MenuService(AppDbContext ctx, IS3Service s3)
    {
        _ctx = ctx;
        _s3 = s3;
    }

    public async Task<List<MealSummaryDto>> GetActiveMealsAsync(string[] userAllergies)
    {
        var meals = await _ctx.Meals.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync();

        return meals
            .Select(m =>
            {
                var matchedAllergens = userAllergies
                    .Where(a =>
                        m.Ingredients != null
                        && m.Ingredients.Any(ing =>
                            ing.Contains(a, StringComparison.OrdinalIgnoreCase)
                        )
                    )
                    .ToArray();

                return new MealSummaryDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    ImageUrls = _s3.ResolveMealImageUrls(m.Images ?? []),
                    Calories = m.Calories,
                    ProteinG = m.Protein,
                    CarbsG = m.Carbs,
                    FatG = m.Fat,
                    Ingredients = m.Ingredients ?? [],
                    HasAllergenWarning = matchedAllergens.Length > 0,
                    MatchedAllergens = matchedAllergens,
                };
            })
            .ToList();
    }

    public async Task SaveDaySelectionsAsync(
        Guid userId,
        int subscriptionId,
        SaveDaySelectionsDto dto
    )
    {
        // Rule: Qua 00:00 ngày giao hàng thì không thể thay đổi món (phải sửa trước 11:59p ngày hôm trước)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dto.Date <= today)
            throw new InvalidOperationException("Không thể thay đổi món đã chọn. Thời hạn chỉnh sửa là trước 00:00 ngày giao hàng.");

        // Get or create the Order for this date
        var order = await _ctx
            .Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o =>
                o.UserId == userId
                && o.SubscriptionId == subscriptionId
                && o.DeliveryDate == dto.Date
            );

        if (order == null)
        {
            order = new Order
            {
                UserId = userId,
                SubscriptionId = subscriptionId,
                DeliveryDate = dto.Date,
                Status = OrderStatus.Planned,
            };
            _ctx.Orders.Add(order);
            await _ctx.SaveChangesAsync();
        }
        else
        {
            // Remove previous selections for this day
            _ctx.OrderItems.RemoveRange(order.Items);
            await _ctx.SaveChangesAsync();
        }

        // Fetch user delivery address snapshot
        var user = await _ctx.Users.FindAsync(userId);
        var addressSnapshot = user?.DeliveryAddress ?? "";

        // Add new selections
        foreach (var sel in dto.Selections)
        {
            _ctx.OrderItems.Add(
                new OrderItem
                {
                    OrderId = order.Id,
                    MealId = sel.MealId,
                    DeliverySlotId = sel.DeliverySlotId,
                    Quantity = 1,
                    Status = OrderItemStatus.Planned,
                    DeliveryAddress = addressSnapshot,
                }
            );
        }

        await _ctx.SaveChangesAsync();
    }

    public async Task<List<(DateOnly Date, List<MealSummaryDto> Meals)>> GetWeekSelectionsAsync(
        Guid userId,
        DateOnly weekStart
    )
    {
        var weekEnd = weekStart.AddDays(6);

        var orders = await _ctx
            .Orders.Include(o => o.Items)
                .ThenInclude(oi => oi.Meal)
            .Where(o =>
                o.UserId == userId && o.DeliveryDate >= weekStart && o.DeliveryDate <= weekEnd
            )
            .ToListAsync();

        return Enumerable
            .Range(0, 7)
            .Select(i =>
            {
                var date = weekStart.AddDays(i);
                var dayOrder = orders.FirstOrDefault(o => o.DeliveryDate == date);
                var meals =
                    dayOrder
                        ?.Items.Select(oi => new MealSummaryDto
                        {
                            Id = oi.MealId,
                            Name = oi.Meal?.Name ?? "",
                            ImageUrls = _s3.ResolveMealImageUrls(oi.Meal?.Images ?? []),
                            Calories = oi.Meal?.Calories ?? 0,
                            ProteinG = oi.Meal?.Protein ?? 0,
                            CarbsG = oi.Meal?.Carbs ?? 0,
                            FatG = oi.Meal?.Fat ?? 0,
                        })
                        .ToList()
                    ?? [];

                return (date, meals);
            })
            .ToList();
    }

    public async Task<List<(DateOnly Date, List<MealSummaryDto> Meals)>> GetSelectionsForDateRangeAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        int? subscriptionId = null
    )
    {
        var query = _ctx.Orders.Include(o => o.Items).ThenInclude(oi => oi.Meal)
            .Where(o => o.UserId == userId && o.DeliveryDate >= startDate && o.DeliveryDate <= endDate);
        if (subscriptionId.HasValue)
            query = query.Where(o => o.SubscriptionId == subscriptionId.Value);
        var orders = await query.ToListAsync();

        var result = new List<(DateOnly, List<MealSummaryDto>)>();
        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
            var dayOrder = orders.FirstOrDefault(o => o.DeliveryDate == d);
            var meals =
                dayOrder
                    ?.Items.Select(oi => new MealSummaryDto
                    {
                        Id = oi.MealId,
                        Name = oi.Meal?.Name ?? "",
                        ImageUrls = _s3.ResolveMealImageUrls(oi.Meal?.Images ?? []),
                        Calories = oi.Meal?.Calories ?? 0,
                        ProteinG = oi.Meal?.Protein ?? 0,
                        CarbsG = oi.Meal?.Carbs ?? 0,
                        FatG = oi.Meal?.Fat ?? 0,
                    })
                    .ToList()
                ?? [];
            result.Add((d, meals));
        }

        return result;
    }

    public async Task<List<DateOnly>> GetRemainingDatesInWeekAsync(
        Guid userId,
        int subscriptionId,
        DateOnly weekStart,
        DateOnly fromDate
    )
    {
        var weekEnd = weekStart.AddDays(6);

        var orders = await _ctx
            .Orders.Include(o => o.Items)
            .Where(o =>
                o.UserId == userId
                && o.SubscriptionId == subscriptionId
                && o.DeliveryDate >= weekStart
                && o.DeliveryDate <= weekEnd
            )
            .ToListAsync();

        var remaining = new List<DateOnly>();

        for (var i = 0; i < 7; i++)
        {
            var date = weekStart.AddDays(i);
            if (date < fromDate)
                continue;

            var order = orders.FirstOrDefault(o => o.DeliveryDate == date);
            var hasItems = order?.Items != null && order.Items.Any();

            if (!hasItems)
            {
                remaining.Add(date);
            }
        }

        return remaining;
    }
}
