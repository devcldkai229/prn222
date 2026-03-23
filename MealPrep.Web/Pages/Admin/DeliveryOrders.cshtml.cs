using BusinessObjects.Enums;
using MealPrep.BLL.Services;
using MealPrep.BLL.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace MealPrep.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DeliveryOrdersModel : PageModel
{
    private readonly IAdminDeliveryOrderService _deliveryService;
    private readonly IHubContext<OrderHub> _orderHub;
    private readonly IHubContext<DashboardHub> _dashboardHub;

    public DeliveryOrdersModel(
        IAdminDeliveryOrderService deliveryService,
        IHubContext<OrderHub> orderHub,
        IHubContext<DashboardHub> dashboardHub)
    {
        _deliveryService = deliveryService;
        _orderHub = orderHub;
        _dashboardHub = dashboardHub;
    }

    [BindProperty(SupportsGet = true)]
    public DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    [BindProperty(SupportsGet = true)]
    public OrderStatus? StatusFilter { get; set; }

    public List<OrderListItemDto> Orders { get; set; } = new();
    public List<ShipperSelectDto> Shippers { get; set; } = new();

    public async Task OnGetAsync()
    {
        Orders = await _deliveryService.GetOrdersAsync(SelectedDate, StatusFilter);
        Shippers = await _deliveryService.GetActiveShippersAsync();
    }

    public async Task<IActionResult> OnPostAssignShipperAsync(int orderId, Guid shipperId)
    {
        try
        {
            await _deliveryService.AssignShipperAsync(orderId, shipperId);
            TempData["Success"] = $"Shipper assigned to Order #{orderId} successfully.";

            // SignalR: Notify all admins and the assigned shipper
            var shippers = await _deliveryService.GetActiveShippersAsync();
            var shipper = shippers.FirstOrDefault(s => s.Id == shipperId);
            var shipperName = shipper?.FullName ?? "Unknown";

            await _orderHub.Clients.Group("Admins")
                .SendAsync("ReceiveShipperAssigned", orderId, shipperName);
            var shipperGroup = $"Shipper_{shipperId:D}".ToLowerInvariant();
            await _orderHub.Clients.Group(shipperGroup)
                .SendAsync("ReceiveNewAssignment", orderId);
            await _dashboardHub.Clients.Group("AdminDashboard")
                .SendAsync("ReceiveDashboardUpdate");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage(new { SelectedDate, StatusFilter });
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, OrderStatus newStatus)
    {
        try
        {
            await _deliveryService.UpdateStatusAsync(orderId, newStatus);
            TempData["Success"] = $"Order #{orderId} status updated.";

            // SignalR: Broadcast status change to admins and the order's user
            await _orderHub.Clients.Group("Admins")
                .SendAsync("ReceiveOrderStatusUpdate", orderId, newStatus.ToString(), "Admin");
            await _orderHub.Clients.All
                .SendAsync("ReceiveOrderStatusUpdate", orderId, newStatus.ToString(), "Admin");
            await _dashboardHub.Clients.Group("AdminDashboard")
                .SendAsync("ReceiveDashboardUpdate");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage(new { SelectedDate, StatusFilter });
    }
}
