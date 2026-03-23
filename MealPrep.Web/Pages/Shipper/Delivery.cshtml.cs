using System.Security.Claims;
using BusinessObjects.Enums;
using MealPrep.BLL.Services;
using MealPrep.BLL.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace MealPrep.Web.Pages.Shipper;

[Authorize(Roles = "Admin,Shipper")]
public class DeliveryModel : PageModel
{
    private readonly IShipperService _shipperService;
    private readonly IHubContext<OrderHub> _orderHub;
    private readonly IHubContext<OrderTrackingHub> _trackingHub;
    private readonly IHubContext<DashboardHub> _dashboardHub;

    public DeliveryModel(
        IShipperService shipperService,
        IHubContext<OrderHub> orderHub,
        IHubContext<OrderTrackingHub> trackingHub,
        IHubContext<DashboardHub> dashboardHub)
    {
        _shipperService = shipperService;
        _orderHub = orderHub;
        _trackingHub = trackingHub;
        _dashboardHub = dashboardHub;
    }

    public List<ShipperOrderDto> TodaysOrders { get; private set; } = [];

    private Guid ShipperId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync()
    {
        TodaysOrders = await _shipperService.GetTodaysOrdersAsync(ShipperId);
    }

    public async Task<IActionResult> OnGetOrderJsonAsync(int orderId)
    {
        var order = await _shipperService.GetOrderByIdAsync(orderId, ShipperId);
        if (order == null) return new JsonResult(new { found = false });
        return new JsonResult(new { found = true, order });
    }

    public async Task<IActionResult> OnPostUploadProofAsync(int orderItemId, IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
            return new JsonResult(new { success = false, error = "No photo received." });

        using var stream = photo.OpenReadStream();
        var (publicUrl, orderId, _, customerUserId) = await _shipperService.UploadDeliveryProofAsync(
            orderItemId,
            ShipperId,
            stream,
            photo.FileName,
            photo.ContentType
        );

        // Broadcast OrderItem status via OrderTrackingHub
        await _trackingHub.Clients.Group("Tracking_Admins")
            .SendAsync("OrderItemStatusChanged", orderItemId, orderId, "Delivered", publicUrl);
        if (customerUserId.HasValue)
        {
            await _trackingHub.Clients.Group($"Tracking_User_{customerUserId}")
                .SendAsync("OrderItemStatusChanged", orderItemId, orderId, "Delivered", publicUrl);
            await _dashboardHub.Clients.Group($"UserDashboard_{customerUserId.Value}".ToLowerInvariant())
                .SendAsync("DashboardDataChanged", new { type = "NewItemsToRate" });
        }
        await _trackingHub.Clients.Group($"Tracking_Shipper_{ShipperId:D}".ToLowerInvariant())
            .SendAsync("OrderItemStatusChanged", orderItemId, orderId, "Delivered", publicUrl);

        return new JsonResult(new { success = true, proofUrl = publicUrl });
    }

    public async Task<IActionResult> OnPostCompleteOrderAsync(int orderId)
    {
        var customerUserId = await _shipperService.CompleteOrderAsync(orderId, ShipperId);

        // Broadcast via OrderHub (existing) — Order.Delivered when shipper completes
        await _orderHub.Clients.Group("Admins").SendAsync("ReceiveOrderStatusUpdate", orderId, "Delivered", "Shipper");
        await _orderHub.Clients.All.SendAsync("ReceiveOrderStatusUpdate", orderId, "Delivered", "Shipper");
        await _dashboardHub.Clients.Group("AdminDashboard").SendAsync("ReceiveDashboardUpdate");

        // Broadcast via OrderTrackingHub
        await _trackingHub.Clients.Group("Tracking_Admins")
            .SendAsync("OrderStatusChanged", orderId, "Delivered", "Shipper");
        if (customerUserId.HasValue)
        {
            await _trackingHub.Clients.Group($"Tracking_User_{customerUserId}")
                .SendAsync("OrderStatusChanged", orderId, "Delivered", "Shipper");
            await _dashboardHub.Clients.Group($"UserDashboard_{customerUserId.Value}".ToLowerInvariant())
                .SendAsync("DashboardDataChanged", new { type = "NewItemsToRate" });
        }

        return new JsonResult(new { success = true });
    }

    public async Task<IActionResult> OnPostUpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        try
        {
            await _shipperService.UpdateOrderStatusAsync(orderId, ShipperId, newStatus);
            await _orderHub.Clients.Group("Admins").SendAsync("ReceiveOrderStatusUpdate", orderId, newStatus.ToString(), "Shipper");
            await _orderHub.Clients.All.SendAsync("ReceiveOrderStatusUpdate", orderId, newStatus.ToString(), "Shipper");
            await _trackingHub.Clients.Group("Tracking_Admins").SendAsync("OrderStatusChanged", orderId, newStatus.ToString(), "Shipper");
            await _trackingHub.Clients.Group($"Tracking_Shipper_{ShipperId}".ToLowerInvariant()).SendAsync("OrderStatusChanged", orderId, newStatus.ToString(), "Shipper");
            var customerUserId = await _shipperService.GetOrderCustomerUserIdAsync(orderId, ShipperId);
            if (customerUserId.HasValue)
                await _trackingHub.Clients.Group($"Tracking_User_{customerUserId.Value}").SendAsync("OrderStatusChanged", orderId, newStatus.ToString(), "Shipper");
            return new JsonResult(new { success = true });
        }
        catch (Exception ex) { return new JsonResult(new { success = false, error = ex.Message }); }
    }

    public async Task<IActionResult> OnPostUpdateOrderItemStatusAsync(int orderItemId, OrderItemStatus newStatus)
    {
        try
        {
            await _shipperService.UpdateOrderItemStatusAsync(orderItemId, ShipperId, newStatus);
            var item = await _shipperService.GetTodaysOrdersAsync(ShipperId);
            var found = item.SelectMany(o => o.Items).FirstOrDefault(i => i.OrderItemId == orderItemId);
            var order = found != null ? item.First(o => o.Items.Contains(found)) : null;
            var orderId = order?.OrderId ?? 0;
            if (orderId > 0)
            {
                await _trackingHub.Clients.Group("Tracking_Admins").SendAsync("OrderItemStatusChanged", orderItemId, orderId, newStatus.ToString(), (string?)null);
                await _trackingHub.Clients.Group($"Tracking_Shipper_{ShipperId}".ToLowerInvariant()).SendAsync("OrderItemStatusChanged", orderItemId, orderId, newStatus.ToString(), (string?)null);
                var customerUserId = await _shipperService.GetOrderCustomerUserIdAsync(orderId, ShipperId);
                if (customerUserId.HasValue)
                {
                    await _trackingHub.Clients.Group($"Tracking_User_{customerUserId.Value}").SendAsync("OrderItemStatusChanged", orderItemId, orderId, newStatus.ToString(), (string?)null);
                    if (newStatus == OrderItemStatus.Delivered)
                        await _dashboardHub.Clients.Group($"UserDashboard_{customerUserId.Value}".ToLowerInvariant())
                            .SendAsync("DashboardDataChanged", new { type = "NewItemsToRate" });
                }
            }
            return new JsonResult(new { success = true });
        }
        catch (Exception ex) { return new JsonResult(new { success = false, error = ex.Message }); }
    }
}
