using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.BLL.Services.Interfaces;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class ShipperOrderDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public string DeliverySlotName { get; set; } = "";
    public OrderStatus Status { get; set; }
    public List<ShipperOrderItemDto> Items { get; set; } = [];
}

public class ShipperOrderItemDto
{
    public int OrderItemId { get; set; }
    public int MealId { get; set; }
    public string MealName { get; set; } = "";
    public string[] MealImageUrls { get; set; } = [];
    public string? DeliveryAddress { get; set; }
    public string[]? ProofImageUrls { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public bool IsDelivered => DeliveredAt.HasValue;
    public OrderItemStatus Status { get; set; }
    public int? DeliverySlotId { get; set; }
    public string DeliverySlotName { get; set; } = "";
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IShipperService
{
    Task<List<ShipperOrderDto>> GetTodaysOrdersAsync(Guid shipperId);
    Task<ShipperOrderDto?> GetOrderByIdAsync(int orderId, Guid shipperId);
    Task UpdateOrderStatusAsync(int orderId, Guid shipperId, OrderStatus newStatus);
    Task UpdateOrderItemStatusAsync(int orderItemId, Guid shipperId, OrderItemStatus newStatus);
    /// <returns>PublicUrl, OrderId, OrderItemId, CustomerUserId (for SignalR broadcast)</returns>
    Task<(string PublicUrl, int OrderId, int OrderItemId, Guid? CustomerUserId)> UploadDeliveryProofAsync(
        int orderItemId,
        Guid shipperId,
        Stream photoStream,
        string fileName,
        string contentType
    );
    /// <returns>Customer UserId for SignalR broadcast</returns>
    Task<Guid?> CompleteOrderAsync(int orderId, Guid shipperId);
    Task<Guid?> GetOrderCustomerUserIdAsync(int orderId, Guid shipperId);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class ShipperService : IShipperService
{
    private readonly AppDbContext _ctx;
    private readonly IS3Service _s3Service;

    public ShipperService(AppDbContext ctx, IS3Service s3Service)
    {
        _ctx = ctx;
        _s3Service = s3Service;
    }

    public async Task<List<ShipperOrderDto>> GetTodaysOrdersAsync(Guid shipperId)
    {
        var orders = await _ctx
            .Orders.Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Meal)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.DeliverySlot)
            .Where(o => o.ShipperId == shipperId && o.Status != OrderStatus.Cancelled)
            .OrderBy(o => o.DeliveryDate)
            .ThenBy(o => o.Items.Select(i => i.DeliverySlot != null ? i.DeliverySlot.Name : null).FirstOrDefault() ?? "")
            .ToListAsync();

        return orders
            .Select(o => new ShipperOrderDto
            {
                OrderId = o.Id,
                CustomerName = o.User?.FullName ?? "Unknown",
                DeliverySlotName = o.Items.FirstOrDefault()?.DeliverySlot?.Name ?? "—",
                Status = o.Status,
                Items = o
                    .Items.OrderBy(oi => oi.DeliverySlot?.Name ?? "")
                    .Select(oi => new ShipperOrderItemDto
                    {
                        OrderItemId = oi.Id,
                        MealId = oi.MealId,
                        MealName = oi.Meal?.Name ?? "",
                        MealImageUrls = _s3Service.ResolveMealImageUrls(oi.Meal?.Images ?? []),
                        DeliveryAddress = oi.DeliveryAddress,
                        ProofImageUrls = oi
                            .ImageS3Keys?.Select(k => _s3Service.GetPresignedUrl(k, 24))
                            .ToArray(),
                        DeliveredAt = oi.DeliveredAt,
                        Status = oi.Status,
                        DeliverySlotId = oi.DeliverySlotId,
                        DeliverySlotName = oi.DeliverySlot?.Name ?? "—",
                    })
                    .ToList(),
            })
            .ToList();
    }

    public async Task<(string PublicUrl, int OrderId, int OrderItemId, Guid? CustomerUserId)> UploadDeliveryProofAsync(
        int orderItemId,
        Guid shipperId,
        Stream photoStream,
        string fileName,
        string contentType
    )
    {
        var orderItem =
            await _ctx
                .OrderItems.Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.Id == orderItemId && oi.Order!.ShipperId == shipperId)
            ?? throw new InvalidOperationException("Order item not found or access denied.");

        var s3Key = await _s3Service.UploadFileAsync(
            photoStream,
            fileName,
            "delivery-proofs",
            contentType
        );

        var existing = orderItem.ImageS3Keys ?? [];
        orderItem.ImageS3Keys = [.. existing, s3Key];
        orderItem.DeliveredAt = DateTime.UtcNow;
        orderItem.Status = OrderItemStatus.Delivered;

        await _ctx.SaveChangesAsync();

        var publicUrl = _s3Service.GetPresignedUrl(s3Key, 24);
        var customerUserId = orderItem.Order?.UserId;
        return (publicUrl, orderItem.OrderId, orderItem.Id, customerUserId);
    }

    public async Task<Guid?> CompleteOrderAsync(int orderId, Guid shipperId)
    {
        var order =
            await _ctx
                .Orders.Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.ShipperId == shipperId)
            ?? throw new InvalidOperationException("Order not found or access denied.");

        var allDelivered = order.Items.All(oi => oi.DeliveredAt.HasValue);
        if (!allDelivered)
            throw new InvalidOperationException("Not all items have been delivered yet.");

        order.Status = OrderStatus.Delivered; // Shipper delivered; user confirms → ConfirmedByUser
        await _ctx.SaveChangesAsync();
        return order.UserId;
    }

    public async Task<ShipperOrderDto?> GetOrderByIdAsync(int orderId, Guid shipperId)
    {
        var order = await _ctx.Orders
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(oi => oi.Meal)
            .Include(o => o.Items).ThenInclude(oi => oi.DeliverySlot)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.ShipperId == shipperId);
        if (order == null) return null;
        return new ShipperOrderDto
        {
            OrderId = order.Id,
            CustomerName = order.User?.FullName ?? "Unknown",
            DeliverySlotName = order.Items.FirstOrDefault()?.DeliverySlot?.Name ?? "—",
            Status = order.Status,
            Items = order.Items.OrderBy(oi => oi.DeliverySlot?.Name ?? "").Select(oi => new ShipperOrderItemDto
            {
                OrderItemId = oi.Id,
                MealId = oi.MealId,
                MealName = oi.Meal?.Name ?? "",
                MealImageUrls = _s3Service.ResolveMealImageUrls(oi.Meal?.Images ?? []),
                DeliveryAddress = oi.DeliveryAddress,
                ProofImageUrls = oi.ImageS3Keys?.Select(k => _s3Service.GetPresignedUrl(k, 24)).ToArray(),
                DeliveredAt = oi.DeliveredAt,
                Status = oi.Status,
                DeliverySlotId = oi.DeliverySlotId,
                DeliverySlotName = oi.DeliverySlot?.Name ?? "—",
            }).ToList(),
        };
    }

    public async Task UpdateOrderStatusAsync(int orderId, Guid shipperId, OrderStatus newStatus)
    {
        var order = await _ctx.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.ShipperId == shipperId)
            ?? throw new InvalidOperationException("Order not found or access denied.");
        order.Status = newStatus;
        await _ctx.SaveChangesAsync();
    }

    public async Task<Guid?> GetOrderCustomerUserIdAsync(int orderId, Guid shipperId)
    {
        var order = await _ctx.Orders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == orderId && x.ShipperId == shipperId);
        return order?.UserId;
    }

    public async Task UpdateOrderItemStatusAsync(int orderItemId, Guid shipperId, OrderItemStatus newStatus)
    {
        var item = await _ctx.OrderItems.Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId && oi.Order!.ShipperId == shipperId)
            ?? throw new InvalidOperationException("Order item not found or access denied.");
        item.Status = newStatus;
        if (newStatus == OrderItemStatus.Delivered)
            item.DeliveredAt ??= DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }
}
