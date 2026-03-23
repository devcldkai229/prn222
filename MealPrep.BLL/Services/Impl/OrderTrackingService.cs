using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.BLL.Services.Interfaces;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class UserOrderDto
{
    public int OrderId { get; set; }
    public int SubscriptionId { get; set; }
    public string? PlanName { get; set; }
    public DateOnly? SubscriptionStart { get; set; }
    public DateOnly? SubscriptionEnd { get; set; }
    public DateTime? SubscriptionCreatedAt { get; set; }
    public DateOnly DeliveryDate { get; set; }
    public string SlotName { get; set; } = "";
    public OrderStatus Status { get; set; }
    public string? ShipperName { get; set; }
    public string? ShipperPhone { get; set; }
    public List<UserOrderItemDto> Items { get; set; } = [];
}

public class UserOrderItemDto
{
    public int OrderItemId { get; set; }
    public int MealId { get; set; }
    public string MealName { get; set; } = "";
    public string[] MealImages { get; set; } = [];
    public OrderItemStatus Status { get; set; }
    public bool IsDelivered => DeliveredAt.HasValue;
    public DateTime? DeliveredAt { get; set; }
    public string[] ProofImageUrls { get; set; } = [];
    public string DeliverySlotName { get; set; } = "";
    public bool HasRated { get; set; }
    public DateOnly DeliveryDate { get; set; }
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IOrderTrackingService
{
    Task<List<UserOrderDto>> GetUserOrdersAsync(Guid userId, int take = 20);
    Task<UserOrderDto?> GetOrderDetailAsync(int orderId, Guid userId);
    Task ConfirmReceiptAsync(int orderId, Guid userId);
    Task ReportMissingOrderAsync(int orderId, Guid userId, string note);
    Task TryAutoConfirmDeliveredOrdersAsync();
}

// ── Implementation ────────────────────────────────────────────────────────────

public class OrderTrackingService : IOrderTrackingService
{
    private readonly AppDbContext _ctx;
    private readonly IS3Service _s3;

    public OrderTrackingService(AppDbContext ctx, IS3Service s3) => (_ctx, _s3) = (ctx, s3);

    public async Task<List<UserOrderDto>> GetUserOrdersAsync(Guid userId, int take = 20)
    {
        var orders = await _ctx
            .Orders.Include(o => o.Shipper)
            .Include(o => o.Subscription).ThenInclude(s => s!.Plan)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Meal)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.DeliverySlot)
            .Where(o => o.UserId == userId)
            .OrderBy(o => o.SubscriptionId)
            .ThenBy(o => o.DeliveryDate)
            .Take(take)
            .ToListAsync();

        var ratedOrderItemIds = await _ctx
            .MealRatings.Where(r => r.UserId == userId)
            .Select(r => r.OrderItemId)
            .ToListAsync();

        return orders
            .Select(o => new UserOrderDto
            {
                OrderId = o.Id,
                SubscriptionId = o.SubscriptionId,
                PlanName = o.Subscription?.Plan?.Name,
                SubscriptionStart = o.Subscription?.StartDate,
                SubscriptionEnd = o.Subscription?.EndDate,
                SubscriptionCreatedAt = o.Subscription?.CreatedAt,
                DeliveryDate = o.DeliveryDate,
                SlotName = o.Items.FirstOrDefault()?.DeliverySlot?.Name ?? "—",
                Status = o.Status,
                ShipperName = o.Shipper?.FullName,
                ShipperPhone = o.Shipper?.PhoneNumber,
                Items = o
                    .Items.OrderBy(oi => oi.DeliverySlot?.Name ?? "")
                    .Select(oi => new UserOrderItemDto
                    {
                        OrderItemId = oi.Id,
                        MealId = oi.MealId,
                        MealName = oi.Meal?.Name ?? "",
                        MealImages = _s3.ResolveMealImageUrls(oi.Meal?.Images ?? []),
                        Status = oi.Status,
                        DeliveredAt = oi.DeliveredAt,
                        ProofImageUrls =
                            oi.ImageS3Keys?.Select(k => _s3.GetPresignedUrl(k, 24)).ToArray() ?? [],
                        DeliverySlotName = oi.DeliverySlot?.Name ?? "—",
                        HasRated = ratedOrderItemIds.Contains(oi.Id),
                        DeliveryDate = o.DeliveryDate,
                    })
                    .ToList(),
            })
            .ToList();
    }

    public async Task<UserOrderDto?> GetOrderDetailAsync(int orderId, Guid userId)
    {
        var orders = await GetUserOrdersAsync(userId, 50);
        return orders.FirstOrDefault(o => o.OrderId == orderId);
    }

    public async Task ConfirmReceiptAsync(int orderId, Guid userId)
    {
        var order =
            await _ctx.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new InvalidOperationException("Order not found.");

        if (order.Status != OrderStatus.Delivered && order.Status != OrderStatus.Disputed)
            throw new InvalidOperationException("Chỉ có thể xác nhận đơn đã giao hoặc đang tranh chấp.");

        order.Status = OrderStatus.ConfirmedByUser;
        await _ctx.SaveChangesAsync();
    }

    public async Task TryAutoConfirmDeliveredOrdersAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var orders = await _ctx.Orders
            .Where(o => o.Status == OrderStatus.Delivered && o.DeliveryDate < today)
            .ToListAsync();
        foreach (var o in orders)
        {
            o.Status = OrderStatus.ConfirmedByUser;
        }
        if (orders.Count > 0)
            await _ctx.SaveChangesAsync();
    }

    public async Task ReportMissingOrderAsync(int orderId, Guid userId, string note)
    {
        var order =
            await _ctx.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new InvalidOperationException("Order not found.");

        // Flag as disputed — store note in subscription's UpdatedAt as a workaround
        // Best practice: add DisputeNote field in a future migration
        order.Status = OrderStatus.Disputed;
        await _ctx.SaveChangesAsync();

        // Log as a NutritionLog is not appropriate — store as PaymentTransaction note
        // For now the status change is sufficient for admin visibility
    }
}
