using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.BLL.Services.Interfaces;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class DisputedOrderDto
{
    public int OrderId { get; set; }
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public DateOnly DeliveryDate { get; set; }
    public Guid? ShipperId { get; set; }
    public string? ShipperName { get; set; }
    public string? ShipperPhone { get; set; }
    public List<DisputedItemDto> Items { get; set; } = [];
}

public class DisputedItemDto
{
    public int OrderItemId { get; set; }
    public string MealName { get; set; } = "";
    public string[] ProofImageUrls { get; set; } = [];
    public DateTime? DeliveredAt { get; set; }
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IAdminSupportService
{
    Task<List<DisputedOrderDto>> GetDisputedOrdersAsync();
    Task<DisputedOrderDto?> GetDisputedOrderAsync(int orderId);
    Task<int> RescheduleMissingOrderAsync(int originalOrderId, DateOnly newDeliveryDate);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class AdminSupportService : IAdminSupportService
{
    private readonly AppDbContext _ctx;
    private readonly IS3Service _s3;

    public AdminSupportService(AppDbContext ctx, IS3Service s3) => (_ctx, _s3) = (ctx, s3);

    public async Task<List<DisputedOrderDto>> GetDisputedOrdersAsync()
    {
        var orders = await _ctx
            .Orders.Include(o => o.User)
            .Include(o => o.Shipper)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Meal)
            .Where(o => o.Status == OrderStatus.Disputed)
            .OrderByDescending(o => o.DeliveryDate)
            .ToListAsync();

        return orders
            .Select(o => new DisputedOrderDto
            {
                OrderId = o.Id,
                UserId = o.UserId,
                CustomerName = o.User?.FullName ?? "—",
                CustomerEmail = o.User?.Email ?? "—",
                DeliveryDate = o.DeliveryDate,
                ShipperId = o.ShipperId,
                ShipperName = o.Shipper?.FullName,
                ShipperPhone = o.Shipper?.PhoneNumber,
                Items = o
                    .Items.Select(oi => new DisputedItemDto
                    {
                        OrderItemId = oi.Id,
                        MealName = oi.Meal?.Name ?? "",
                        ProofImageUrls =
                            oi.ImageS3Keys?.Select(k => _s3.GetPresignedUrl(k, 24)).ToArray() ?? [],
                        DeliveredAt = oi.DeliveredAt,
                    })
                    .ToList(),
            })
            .ToList();
    }

    public async Task<DisputedOrderDto?> GetDisputedOrderAsync(int orderId)
    {
        var order = await _ctx
            .Orders.Include(o => o.User)
            .Include(o => o.Shipper)
            .Include(o => o.Items).ThenInclude(oi => oi.Meal)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Status == OrderStatus.Disputed);
        if (order == null) return null;
        return new DisputedOrderDto
        {
            OrderId = order.Id,
            UserId = order.UserId,
            CustomerName = order.User?.FullName ?? "—",
            CustomerEmail = order.User?.Email ?? "—",
            DeliveryDate = order.DeliveryDate,
            ShipperId = order.ShipperId,
            ShipperName = order.Shipper?.FullName,
            ShipperPhone = order.Shipper?.PhoneNumber,
            Items = order
                .Items.Select(oi => new DisputedItemDto
                {
                    OrderItemId = oi.Id,
                    MealName = oi.Meal?.Name ?? "",
                    ProofImageUrls = oi.ImageS3Keys?.Select(k => _s3.GetPresignedUrl(k, 24)).ToArray() ?? [],
                    DeliveredAt = oi.DeliveredAt,
                })
                .ToList(),
        };
    }

    public async Task<int> RescheduleMissingOrderAsync(
        int originalOrderId,
        DateOnly newDeliveryDate
    )
    {
        var original =
            await _ctx
                .Orders.Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == originalOrderId)
            ?? throw new InvalidOperationException("Order not found.");

        // Clone order with new date & reset state
        var replacement = new Order
        {
            UserId = original.UserId,
            SubscriptionId = original.SubscriptionId,
            DeliveryDate = newDeliveryDate,
            ShipperId = null, // will be assigned later
            Status = OrderStatus.Planned,
        };
        _ctx.Orders.Add(replacement);
        await _ctx.SaveChangesAsync();

        // Clone order items, clearing delivery proof fields
        foreach (var item in original.Items)
        {
            _ctx.OrderItems.Add(
                new OrderItem
                {
                    OrderId = replacement.Id,
                    MealId = item.MealId,
                    DeliverySlotId = item.DeliverySlotId,
                    Quantity = item.Quantity,
                    Status = OrderItemStatus.Planned,
                    DeliveryAddress = item.DeliveryAddress,
                    ImageS3Keys = null, // cleared
                    DeliveredAt = null, // cleared
                }
            );
        }

        // Mark original as Cancelled (resolved via compensation)
        original.Status = OrderStatus.Cancelled;

        await _ctx.SaveChangesAsync();
        return replacement.Id;
    }
}
