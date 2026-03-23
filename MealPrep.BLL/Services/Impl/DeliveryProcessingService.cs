using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.BLL.Services.Interfaces;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record GenerateOrdersResult(
    int Generated,
    int Skipped,
    int AutoFilled,
    int Errors,
    List<string> ErrorMessages
);

public class KitchenPrepItem
{
    public int MealId { get; set; }
    public string MealName { get; set; } = "";
    public int TotalQuantity { get; set; }
    public string[] ImageUrls { get; set; } = [];
}

public class DeliveryOrderItemDetailDto
{
    public int OrderItemId { get; set; }
    public string MealName { get; set; } = "";
    public string DeliverySlotName { get; set; } = "";
    public OrderItemStatus Status { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string[] ProofImageUrls { get; set; } = [];
}

public class DeliveryOrderDetailDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string DeliveryAddress { get; set; } = "";
    public DateOnly DeliveryDate { get; set; }
    public OrderStatus Status { get; set; }
    public string? ShipperName { get; set; }
    public List<DeliveryOrderItemDetailDto> Items { get; set; } = [];
}

public class DeliveryOrderSummaryDto
{
    public int OrderId { get; set; }
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string DeliveryAddress { get; set; } = "";
    public DateOnly DeliveryDate { get; set; }
    public OrderStatus Status { get; set; }
    public Guid? ShipperId { get; set; }
    public string? ShipperName { get; set; }
    public string? SlotName { get; set; }
    public int ItemCount { get; set; }
    public bool IsAutoFilled { get; set; }
    public List<string> MealNames { get; set; } = [];
}

public class UserOrderSummaryDto
{
    public int OrderId { get; set; }
    public DateOnly DeliveryDate { get; set; }
    public OrderStatus Status { get; set; }
    public string? SlotName { get; set; }
    public List<string> MealNames { get; set; } = [];
    public bool IsAutoFilled { get; set; }
    public bool CanConfirm => Status == OrderStatus.Delivered;
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IDeliveryProcessingService
{
    // Admin: generate orders
    Task<GenerateOrdersResult> GenerateDeliveryOrdersForDateAsync(DateOnly targetDate);

    // Admin: query + status management
    Task<List<DeliveryOrderSummaryDto>> GetDeliveryOrdersByDateAsync(DateOnly date, OrderStatus? statusFilter = null);
    Task<DeliveryOrderDetailDto?> GetOrderDetailForAdminAsync(int orderId);
    Task UpdateStatusAsync(int orderId, OrderStatus newStatus);
    Task<int> BulkUpdateStatusAsync(List<int> orderIds, OrderStatus newStatus);
    Task<(Guid UserId, Guid? ShipperId)?> GetOrderBroadcastInfoAsync(int orderId);

    // Admin: kitchen export
    Task<List<KitchenPrepItem>> GetKitchenPrepListAsync(DateOnly targetDate);

    // User: view own orders + confirm receipt
    Task<List<UserOrderSummaryDto>> GetOrdersForSubscriptionAsync(int subId, Guid userId);
    Task<bool> ConfirmReceiptAsync(int orderId, Guid userId);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class DeliveryProcessingService : IDeliveryProcessingService
{
    private readonly AppDbContext _ctx;
    private readonly ILogger<DeliveryProcessingService> _logger;
    private readonly INutritionLogService _nutritionSvc;
    private readonly IS3Service _s3;

    public DeliveryProcessingService(
        AppDbContext ctx,
        ILogger<DeliveryProcessingService> logger,
        INutritionLogService nutritionSvc,
        IS3Service s3)
    {
        _ctx = ctx;
        _logger = logger;
        _nutritionSvc = nutritionSvc;
        _s3 = s3;
    }

    // ── Auto-log nutrition when an order is marked Delivered ──────────────────
    private async Task AutoLogNutritionAsync(Order order)
    {
        // Load items with meals if not already loaded
        var items = order.Items.Any()
            ? order.Items
            : (ICollection<OrderItem>)await _ctx.OrderItems
                .Include(i => i.Meal)
                .Where(i => i.OrderId == order.Id)
                .ToListAsync();

        foreach (var item in items)
        {
            if (item.MealId == 0) continue;

            // Skip if already logged for this user + date + meal (idempotent)
            var alreadyLogged = await _ctx.NutritionLogs.AnyAsync(l =>
                l.UserId == order.UserId
                && l.Date == order.DeliveryDate
                && l.MealId == item.MealId);

            if (alreadyLogged) continue;

            await _nutritionSvc.LogMealAsync(
                order.UserId,
                item.MealId,
                order.DeliveryDate,
                item.Quantity > 0 ? item.Quantity : 1);
        }

        _logger.LogInformation(
            "Auto-logged nutrition for Order #{Id} (user {UserId}, date {Date})",
            order.Id, order.UserId, order.DeliveryDate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GENERATE DELIVERY ORDERS
    // Flow:
    //   1. Lấy tất cả Subscription Active có khoảng thời gian bao gồm targetDate
    //   2. Với mỗi sub:
    //      a. Skip nếu đã có Order cho ngày đó
    //      b. Nếu User đã chọn món → copy từ order items hiện tại
    //      c. Nếu chưa chọn → auto-fill từ Published WeeklyMenu
    //         (nếu không có menu → lấy ngẫu nhiên từ active meals)
    //   3. Tạo Order với status = Planned
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<GenerateOrdersResult> GenerateDeliveryOrdersForDateAsync(DateOnly targetDate)
    {
        _logger.LogInformation("Generating delivery orders for {Date}", targetDate);

        var activeSubscriptions = await _ctx.Subscriptions
            .Include(s => s.User)
            .Include(s => s.Plan)
            .Where(s =>
                s.Status == SubscriptionStatus.Active
                && s.StartDate <= targetDate
                && (s.EndDate == null || s.EndDate >= targetDate))
            .ToListAsync();

        // Pre-load published admin WeeklyMenu covering targetDate
        var publishedMenu = await _ctx.WeeklyMenus
            .Include(m => m.Items).ThenInclude(i => i.Meal)
            .Where(m =>
                m.UserId == null        // admin global menu
                && m.IsPublished
                && m.WeekStart <= targetDate
                && m.WeekEnd >= targetDate)
            .OrderByDescending(m => m.WeekStart)
            .FirstOrDefaultAsync();

        // Fallback pool: best-selling active meals
        var fallbackMeals = await _ctx.Meals
            .Where(m => m.IsActive)
            .OrderByDescending(m => m.Id)
            .Take(10)
            .ToListAsync();

        int generated = 0, skipped = 0, autoFilled = 0, errors = 0;
        var errorMessages = new List<string>();

        foreach (var subscription in activeSubscriptions)
        {
            try
            {
                // Skip if order already exists for this sub+date
                var existingOrder = await _ctx.Orders.AnyAsync(o =>
                    o.SubscriptionId == subscription.Id && o.DeliveryDate == targetDate);

                if (existingOrder) { skipped++; continue; }

                var order = new Order
                {
                    UserId = subscription.UserId,
                    SubscriptionId = subscription.Id,
                    DeliveryDate = targetDate,
                    Status = OrderStatus.Planned,
                };
                _ctx.Orders.Add(order);
                await _ctx.SaveChangesAsync();

                // Check if user already selected meals for this date
                var userSelectedItems = await _ctx.OrderItems
                    .Include(oi => oi.Order)
                    .Where(oi =>
                        oi.Order!.UserId == subscription.UserId
                        && oi.Order.SubscriptionId == subscription.Id
                        && oi.Order.DeliveryDate == targetDate
                        && oi.Order.Id != order.Id) // exclude the new empty order
                    .ToListAsync();

                bool wasAutoFilled = false;

                if (userSelectedItems.Any())
                {
                    // Copy user-selected meals (giữ DeliverySlotId nếu có)
                    foreach (var item in userSelectedItems)
                    {
                        _ctx.OrderItems.Add(new OrderItem
                        {
                            OrderId = order.Id,
                            MealId = item.MealId,
                            Quantity = 1,
                            Status = OrderItemStatus.Planned,
                            DeliveryAddress = subscription.User?.DeliveryAddress ?? "",
                            DeliverySlotId = item.DeliverySlotId,
                        });
                    }
                }
                else
                {
                    // ── Auto-fill ─────────────────────────────────────────────
                    // 1st choice: published admin WeeklyMenu for that day
                    int dow = targetDate.DayOfWeek == DayOfWeek.Sunday
                        ? 7
                        : (int)targetDate.DayOfWeek;

                    List<Meal> mealsToUse = [];

                    if (publishedMenu != null)
                    {
                        mealsToUse = publishedMenu.Items
                            .Where(i => i.DayOfWeek == dow && i.Meal != null)
                            .Select(i => i.Meal!)
                            .Take(subscription.MealsPerDay)
                            .ToList();
                    }

                    // 2nd choice: random from fallback pool
                    if (!mealsToUse.Any() && fallbackMeals.Any())
                    {
                        var rng = new Random();
                        mealsToUse = fallbackMeals
                            .OrderBy(_ => rng.Next())
                            .Take(subscription.MealsPerDay)
                            .ToList();
                    }

                    // Bữa 1 → Morning (1), Bữa 2 → Evening (3), Bữa 3 → Afternoon (2)
                    var slotMap = new[] { 1, 3, 2 };
                    for (var i = 0; i < mealsToUse.Count; i++)
                    {
                        var meal = mealsToUse[i];
                        _ctx.OrderItems.Add(new OrderItem
                        {
                            OrderId = order.Id,
                            MealId = meal.Id,
                            Quantity = 1,
                            Status = OrderItemStatus.Planned,
                            DeliveryAddress = subscription.User?.DeliveryAddress ?? "",
                            DeliverySlotId = i < slotMap.Length ? slotMap[i] : 1,
                        });
                    }

                    wasAutoFilled = true;
                }

                order.IsAutoFilled = wasAutoFilled;
                await _ctx.SaveChangesAsync();

                generated++;
                if (wasAutoFilled) autoFilled++;

                _logger.LogInformation(
                    "Created Order #{Id} for Sub #{SubId} (autoFilled={Auto})",
                    order.Id, subscription.Id, wasAutoFilled);
            }
            catch (Exception ex)
            {
                errors++;
                errorMessages.Add($"Sub #{subscription.Id}: {ex.Message}");
                _logger.LogError(ex, "Error processing Subscription {SubId}", subscription.Id);
            }
        }

        return new GenerateOrdersResult(generated, skipped, autoFilled, errors, errorMessages);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // QUERY ORDERS BY DATE (Admin)
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<List<DeliveryOrderSummaryDto>> GetDeliveryOrdersByDateAsync(
        DateOnly date,
        OrderStatus? statusFilter = null)
    {
        var query = _ctx.Orders
            .Include(o => o.User)
            .Include(o => o.Shipper)
            .Include(o => o.Items).ThenInclude(i => i.Meal)
            .Include(o => o.Items).ThenInclude(i => i.DeliverySlot)
            .Where(o => o.DeliveryDate == date && o.Status != OrderStatus.Cancelled);

        if (statusFilter.HasValue)
            query = query.Where(o => o.Status == statusFilter.Value);

        var orders = await query.OrderBy(o => o.Status).ThenBy(o => o.Id).ToListAsync();

        return orders.Select(o => new DeliveryOrderSummaryDto
        {
            OrderId = o.Id,
            UserId = o.UserId,
            CustomerName = o.User?.FullName ?? "Unknown",
            CustomerEmail = o.User?.Email ?? "",
            DeliveryAddress = o.Items.FirstOrDefault()?.DeliveryAddress ?? o.User?.DeliveryAddress ?? "",
            DeliveryDate = o.DeliveryDate,
            Status = o.Status,
            ShipperId = o.ShipperId,
            ShipperName = o.Shipper?.FullName,
            SlotName = o.Items.FirstOrDefault()?.DeliverySlot?.Name,
            ItemCount = o.Items.Count,
            MealNames = o.Items.Where(i => i.Meal != null).Select(i => i.Meal!.Name).ToList(),
        }).ToList();
    }

    public async Task<DeliveryOrderDetailDto?> GetOrderDetailForAdminAsync(int orderId)
    {
        var o = await _ctx.Orders
            .Include(x => x.User)
            .Include(x => x.Shipper)
            .Include(x => x.Items).ThenInclude(i => i.Meal)
            .Include(x => x.Items).ThenInclude(i => i.DeliverySlot)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == orderId);
        if (o == null) return null;
        return new DeliveryOrderDetailDto
        {
            OrderId = o.Id,
            CustomerName = o.User?.FullName ?? "Unknown",
            CustomerEmail = o.User?.Email ?? "",
            DeliveryAddress = o.Items.FirstOrDefault()?.DeliveryAddress ?? o.User?.DeliveryAddress ?? "",
            DeliveryDate = o.DeliveryDate,
            Status = o.Status,
            ShipperName = o.Shipper?.FullName,
            Items = o.Items.OrderBy(i => i.DeliverySlot?.Name ?? "").Select(i => new DeliveryOrderItemDetailDto
            {
                OrderItemId = i.Id,
                MealName = i.Meal?.Name ?? "",
                DeliverySlotName = i.DeliverySlot?.Name ?? "—",
                Status = i.Status,
                DeliveredAt = i.DeliveredAt,
                ProofImageUrls = (i.ImageS3Keys?.Select(k => _s3.GetPresignedUrl(k, 24)).ToArray()) ?? [],
            }).ToList(),
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STATUS MANAGEMENT (Admin)
    // Flow: Planned → Preparing → Delivering → Delivered
    // ─────────────────────────────────────────────────────────────────────────
    public async Task UpdateStatusAsync(int orderId, OrderStatus newStatus)
    {
        var order = await _ctx.Orders
            .Include(o => o.Items).ThenInclude(i => i.Meal)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new InvalidOperationException($"Không tìm thấy đơn hàng #{orderId}.");

        order.Status = newStatus;
        await _ctx.SaveChangesAsync();

        // Auto-log nutrition when order reaches Delivered
        if (newStatus == OrderStatus.Delivered)
            await AutoLogNutritionAsync(order);

        _logger.LogInformation("Order #{Id} status → {Status}", orderId, newStatus);
    }

    public async Task<int> BulkUpdateStatusAsync(List<int> orderIds, OrderStatus newStatus)
    {
        var orders = await _ctx.Orders
            .Include(o => o.Items).ThenInclude(i => i.Meal)
            .Where(o => orderIds.Contains(o.Id))
            .ToListAsync();

        foreach (var o in orders)
            o.Status = newStatus;

        await _ctx.SaveChangesAsync();

        // Auto-log nutrition for all orders that reached Delivered
        if (newStatus == OrderStatus.Delivered)
        {
            foreach (var o in orders)
                await AutoLogNutritionAsync(o);
        }

        _logger.LogInformation("Bulk updated {Count} orders → {Status}", orders.Count, newStatus);
        return orders.Count;
    }

    public async Task<(Guid UserId, Guid? ShipperId)?> GetOrderBroadcastInfoAsync(int orderId)
    {
        var o = await _ctx.Orders.AsNoTracking()
            .Where(x => x.Id == orderId)
            .Select(x => new { x.UserId, x.ShipperId })
            .FirstOrDefaultAsync();
        return o == null ? null : (o.UserId, o.ShipperId);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // KITCHEN PREP LIST EXPORT
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<List<KitchenPrepItem>> GetKitchenPrepListAsync(DateOnly targetDate)
    {
        var raw = await _ctx.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Meal)
            .Where(oi =>
                oi.Order!.DeliveryDate == targetDate
                && oi.Order.Status != OrderStatus.Cancelled
                && oi.Meal != null)
            .GroupBy(oi => new { oi.MealId, oi.Meal!.Name, Images = oi.Meal.Images })
            .Select(g => new { g.Key.MealId, g.Key.Name, g.Key.Images, TotalQuantity = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.TotalQuantity)
            .ToListAsync();
        return raw.Select(x => new KitchenPrepItem
        {
            MealId = x.MealId,
            MealName = x.Name,
            TotalQuantity = x.TotalQuantity,
            ImageUrls = _s3.ResolveMealImageUrls(x.Images ?? []),
        }).ToList();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // USER: VIEW OWN ORDERS FOR A SUBSCRIPTION
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<List<UserOrderSummaryDto>> GetOrdersForSubscriptionAsync(int subId, Guid userId)
    {
        return await _ctx.Orders
            .Include(o => o.Items).ThenInclude(i => i.Meal)
            .Include(o => o.Items).ThenInclude(i => i.DeliverySlot)
            .Where(o => o.SubscriptionId == subId && o.UserId == userId)
            .OrderByDescending(o => o.DeliveryDate)
            .Select(o => new UserOrderSummaryDto
            {
                OrderId = o.Id,
                DeliveryDate = o.DeliveryDate,
                Status = o.Status,
                SlotName = o.Items.Select(i => i.DeliverySlot != null ? i.DeliverySlot.Name : null).FirstOrDefault(),
                IsAutoFilled = o.IsAutoFilled,
                MealNames = o.Items
                    .Where(i => i.Meal != null)
                    .Select(i => i.Meal!.Name)
                    .ToList(),
            })
            .ToListAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // USER: CONFIRM RECEIPT
    // Chỉ cho phép khi order status = Delivered
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<bool> ConfirmReceiptAsync(int orderId, Guid userId)
    {
        var order = await _ctx.Orders.FindAsync(orderId);

        if (order == null || order.UserId != userId)
            return false;

        if (order.Status != OrderStatus.Delivered)
            return false;

        order.Status = OrderStatus.ConfirmedByUser;
        await _ctx.SaveChangesAsync();

        _logger.LogInformation("User {UserId} confirmed receipt of Order #{OrderId}", userId, orderId);
        return true;
    }
}
