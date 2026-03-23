using System.Security.Claims;
using MealPrep.BLL.Services;
using MealPrep.BLL.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace MealPrep.Web.Pages.Dashboard;

[Authorize]
public class OrdersModel : PageModel
{
    private readonly IOrderTrackingService _trackingService;
    private readonly IDeliveryProcessingService _deliverySvc;
    private readonly IHubContext<OrderHub> _orderHub;
    private readonly IHubContext<OrderTrackingHub> _trackingHub;
    private readonly IHubContext<DashboardHub> _dashboardHub;

    public OrdersModel(
        IOrderTrackingService trackingService,
        IDeliveryProcessingService deliverySvc,
        IHubContext<OrderHub> orderHub,
        IHubContext<OrderTrackingHub> trackingHub,
        IHubContext<DashboardHub> dashboardHub)
    {
        _trackingService = trackingService;
        _deliverySvc = deliverySvc;
        _orderHub = orderHub;
        _trackingHub = trackingHub;
        _dashboardHub = dashboardHub;
    }

    public List<UserOrderDto> Orders { get; private set; } = [];

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync()
    {
        await _trackingService.TryAutoConfirmDeliveredOrdersAsync();
        Orders = await _trackingService.GetUserOrdersAsync(UserId);
    }

    public async Task<IActionResult> OnPostConfirmReceiptAsync(int orderId)
    {
        await _trackingService.ConfirmReceiptAsync(orderId, UserId);

        await _orderHub.Clients.Group("Admins").SendAsync("ReceiveOrderStatusUpdate", orderId, "ConfirmedByUser", "Customer");
        await _orderHub.Clients.All.SendAsync("ReceiveOrderStatusUpdate", orderId, "ConfirmedByUser", "Customer");
        await _trackingHub.Clients.Group("Tracking_Admins").SendAsync("OrderStatusChanged", orderId, "ConfirmedByUser", "Customer");
        await _trackingHub.Clients.Group($"Tracking_User_{UserId}").SendAsync("OrderStatusChanged", orderId, "ConfirmedByUser", "Customer");
        var info = await _deliverySvc.GetOrderBroadcastInfoAsync(orderId);
        if (info.HasValue && info.Value.ShipperId.HasValue)
            await _trackingHub.Clients.Group($"Tracking_Shipper_{info.Value.ShipperId:D}".ToLowerInvariant()).SendAsync("OrderStatusChanged", orderId, "ConfirmedByUser", "Customer");
        await _dashboardHub.Clients.Group("AdminDashboard").SendAsync("ReceiveDashboardUpdate");

        return new JsonResult(new { success = true, message = "Đã xác nhận nhận hàng! 🎉" });
    }

    public async Task<IActionResult> OnPostReportMissingAsync(int orderId, string note)
    {
        await _trackingService.ReportMissingOrderAsync(orderId, UserId, note);

        await _orderHub.Clients.Group("Admins").SendAsync("ReceiveOrderStatusUpdate", orderId, "Disputed", "Customer");
        await _orderHub.Clients.All.SendAsync("ReceiveOrderStatusUpdate", orderId, "Disputed", "Customer");
        await _trackingHub.Clients.Group("Tracking_Admins").SendAsync("OrderStatusChanged", orderId, "Disputed", "Customer");
        await _trackingHub.Clients.Group($"Tracking_User_{UserId}").SendAsync("OrderStatusChanged", orderId, "Disputed", "Customer");
        var info = await _deliverySvc.GetOrderBroadcastInfoAsync(orderId);
        if (info.HasValue && info.Value.ShipperId.HasValue)
            await _trackingHub.Clients.Group($"Tracking_Shipper_{info.Value.ShipperId:D}".ToLowerInvariant()).SendAsync("OrderStatusChanged", orderId, "Disputed", "Customer");
        await _dashboardHub.Clients.Group("AdminDashboard").SendAsync("ReceiveDashboardUpdate");

        return new JsonResult(
            new
            {
                success = true,
                message = "Report submitted. Our team will resolve this shortly.",
            }
        );
    }
}
