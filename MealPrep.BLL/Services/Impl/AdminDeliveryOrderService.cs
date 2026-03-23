using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class OrderListItemDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string DeliveryAddress { get; set; } = "";
    public DateOnly DeliveryDate { get; set; }
    public OrderStatus Status { get; set; }
    public Guid? ShipperId { get; set; }
    public string? ShipperName { get; set; }
    public string? SlotName { get; set; }
    public int ItemCount { get; set; }
}

public class ShipperSelectDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IAdminDeliveryOrderService
{
    Task<List<OrderListItemDto>> GetOrdersAsync(DateOnly date, OrderStatus? statusFilter);
    Task AssignShipperAsync(int orderId, Guid shipperId);
    Task UnassignShipperAsync(int orderId);
    Task UpdateStatusAsync(int orderId, OrderStatus newStatus);
    Task<List<ShipperSelectDto>> GetActiveShippersAsync();
}

// ── Implementation ────────────────────────────────────────────────────────────

public class AdminDeliveryOrderService : IAdminDeliveryOrderService
{
    private readonly AppDbContext _ctx;

    public AdminDeliveryOrderService(AppDbContext ctx) => _ctx = ctx;

    public async Task<List<OrderListItemDto>> GetOrdersAsync(
        DateOnly date,
        OrderStatus? statusFilter
    )
    {
        var query = _ctx
            .Orders.Include(o => o.User)
            .Include(o => o.Shipper)
            .Include(o => o.Items).ThenInclude(i => i.DeliverySlot)
            .Where(o => o.DeliveryDate == date);

        if (statusFilter.HasValue)
            query = query.Where(o => o.Status == statusFilter.Value);

        var orders = await query.OrderBy(o => o.Id).ToListAsync();

        return orders
            .Select(o => new OrderListItemDto
            {
                OrderId = o.Id,
                CustomerName = o.User?.FullName ?? "Unknown",
                CustomerEmail = o.User?.Email ?? "",
                DeliveryAddress =
                    o.Items.FirstOrDefault()?.DeliveryAddress ?? o.User?.DeliveryAddress ?? "",
                DeliveryDate = o.DeliveryDate,
                Status = o.Status,
                ShipperId = o.ShipperId,
                ShipperName = o.Shipper?.FullName,
                SlotName = o.Items.FirstOrDefault()?.DeliverySlot?.Name,
                ItemCount = o.Items.Count,
            })
            .ToList();
    }

    public async Task AssignShipperAsync(int orderId, Guid shipperId)
    {
        var order =
            await _ctx.Orders.FindAsync(orderId)
            ?? throw new InvalidOperationException($"Order {orderId} not found.");
        order.ShipperId = shipperId;
        order.Status = OrderStatus.InProgress;
        await _ctx.SaveChangesAsync();
    }

    public async Task UnassignShipperAsync(int orderId)
    {
        var order =
            await _ctx.Orders.FindAsync(orderId)
            ?? throw new InvalidOperationException($"Order {orderId} not found.");
        order.ShipperId = null;
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(int orderId, OrderStatus newStatus)
    {
        var order =
            await _ctx.Orders.FindAsync(orderId)
            ?? throw new InvalidOperationException($"Order {orderId} not found.");

        // Business rule: Cannot mark as Completed if delivery date is in the future
        if (
            newStatus == OrderStatus.Completed
            && order.DeliveryDate > DateOnly.FromDateTime(DateTime.UtcNow)
        )
            throw new InvalidOperationException(
                "Cannot mark an order as Completed before its delivery date."
            );

        order.Status = newStatus;
        await _ctx.SaveChangesAsync();
    }

    public async Task<List<ShipperSelectDto>> GetActiveShippersAsync()
    {
        // Shippers: Users with Role.Shipper, active
        return await _ctx
            .Users.Include(u => u.AppRole)
            .Where(u => u.IsActive && u.AppRole.Name == BusinessObjects.Enums.Role.Shipper)
            .Select(u => new ShipperSelectDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
            })
            .ToListAsync();
    }
}
