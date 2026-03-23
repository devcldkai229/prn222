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
public class OrderDetailModel : PageModel
{
    private readonly IShipperService _shipperService;
    private readonly IHubContext<OrderTrackingHub> _trackingHub;
    private readonly IHubContext<DashboardHub> _dashboardHub;

    public OrderDetailModel(IShipperService shipperService, IHubContext<OrderTrackingHub> trackingHub, IHubContext<DashboardHub> dashboardHub)
    {
        _shipperService = shipperService;
        _trackingHub = trackingHub;
        _dashboardHub = dashboardHub;
    }

    public ShipperOrderDto? Order { get; set; }
    private Guid ShipperId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync(int? id)
    {
        if (id.HasValue)
            Order = await _shipperService.GetOrderByIdAsync(id.Value, ShipperId);
    }

    public async Task<IActionResult> OnPostUpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        await _shipperService.UpdateOrderStatusAsync(orderId, ShipperId, newStatus);
        await _trackingHub.Clients.Group("Tracking_Admins").SendAsync("OrderStatusChanged", orderId, newStatus.ToString(), "Shipper");
        await _trackingHub.Clients.Group($"Tracking_Shipper_{ShipperId}".ToLowerInvariant()).SendAsync("OrderStatusChanged", orderId, newStatus.ToString(), "Shipper");
        var customerUserId = await _shipperService.GetOrderCustomerUserIdAsync(orderId, ShipperId);
        if (customerUserId.HasValue)
            await _trackingHub.Clients.Group($"Tracking_User_{customerUserId.Value}").SendAsync("OrderStatusChanged", orderId, newStatus.ToString(), "Shipper");
        return RedirectToPage(new { id = orderId });
    }

    public async Task<IActionResult> OnPostUpdateOrderItemStatusAsync(int orderItemId, OrderItemStatus newStatus)
    {
        await _shipperService.UpdateOrderItemStatusAsync(orderItemId, ShipperId, newStatus);
        var orders = await _shipperService.GetTodaysOrdersAsync(ShipperId);
        var order = orders.FirstOrDefault(o => o.Items.Any(i => i.OrderItemId == orderItemId));
        if (order != null)
        {
            await _trackingHub.Clients.Group("Tracking_Admins").SendAsync("OrderItemStatusChanged", orderItemId, order.OrderId, newStatus.ToString(), (string?)null);
            await _trackingHub.Clients.Group($"Tracking_Shipper_{ShipperId}".ToLowerInvariant()).SendAsync("OrderItemStatusChanged", orderItemId, order.OrderId, newStatus.ToString(), (string?)null);
            var customerUserId = await _shipperService.GetOrderCustomerUserIdAsync(order.OrderId, ShipperId);
            if (customerUserId.HasValue)
            {
                await _trackingHub.Clients.Group($"Tracking_User_{customerUserId.Value}").SendAsync("OrderItemStatusChanged", orderItemId, order.OrderId, newStatus.ToString(), (string?)null);
                if (newStatus == OrderItemStatus.Delivered)
                    await _dashboardHub.Clients.Group($"UserDashboard_{customerUserId.Value}".ToLowerInvariant())
                        .SendAsync("DashboardDataChanged", new { type = "NewItemsToRate" });
            }
        }
        return RedirectToPage(new { id = order?.OrderId ?? 0 });
    }

    public async Task<IActionResult> OnPostUploadProofAsync(int orderItemId, IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
            return new JsonResult(new { success = false, error = "No photo." });
        using var stream = photo.OpenReadStream();
        var (publicUrl, orderId, _, customerUserId) = await _shipperService.UploadDeliveryProofAsync(orderItemId, ShipperId, stream, photo.FileName, photo.ContentType);
        await _trackingHub.Clients.Group("Tracking_Admins").SendAsync("OrderItemStatusChanged", orderItemId, orderId, "Delivered", publicUrl);
        await _trackingHub.Clients.Group($"Tracking_Shipper_{ShipperId}".ToLowerInvariant()).SendAsync("OrderItemStatusChanged", orderItemId, orderId, "Delivered", publicUrl);
        if (customerUserId.HasValue)
        {
            await _trackingHub.Clients.Group($"Tracking_User_{customerUserId}").SendAsync("OrderItemStatusChanged", orderItemId, orderId, "Delivered", publicUrl);
            await _dashboardHub.Clients.Group($"UserDashboard_{customerUserId.Value}".ToLowerInvariant())
                .SendAsync("DashboardDataChanged", new { type = "NewItemsToRate" });
        }
        return new JsonResult(new { success = true, proofUrl = publicUrl });
    }
}
