using System.Text;
using BusinessObjects.Enums;
using MealPrep.BLL.Services;
using MealPrep.BLL.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace MealPrep.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DeliveryProcessingModel : PageModel
{
    private readonly IDeliveryProcessingService _svc;
    private readonly IAdminDeliveryOrderService _deliveryService;
    private readonly IHubContext<OrderHub> _orderHub;
    private readonly IHubContext<OrderTrackingHub> _trackingHub;

    public DeliveryProcessingModel(
        IDeliveryProcessingService svc,
        IAdminDeliveryOrderService deliveryService,
        IHubContext<OrderHub> orderHub,
        IHubContext<OrderTrackingHub> trackingHub)
    {
        _svc = svc;
        _deliveryService = deliveryService;
        _orderHub = orderHub;
        _trackingHub = trackingHub;
    }

    [BindProperty(SupportsGet = true)]
    public DateOnly TargetDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    [BindProperty(SupportsGet = true)]
    public OrderStatus? StatusFilter { get; set; }

    public List<DeliveryOrderSummaryDto> Orders { get; set; } = [];
    public List<ShipperSelectDto> Shippers { get; set; } = [];
    public List<KitchenPrepItem> KitchenItems { get; set; } = [];

    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        Orders = await _svc.GetDeliveryOrdersByDateAsync(TargetDate, StatusFilter);
        Shippers = await _deliveryService.GetActiveShippersAsync();
        KitchenItems = await _svc.GetKitchenPrepListAsync(TargetDate);
    }

    public async Task<IActionResult> OnGetOrderDetailAsync(int orderId)
    {
        var detail = await _svc.GetOrderDetailForAdminAsync(orderId);
        if (detail == null)
            return new JsonResult((object?)null);
        return new JsonResult(new
        {
            customerName = detail.CustomerName,
            customerEmail = detail.CustomerEmail,
            deliveryAddress = detail.DeliveryAddress,
            deliveryDate = detail.DeliveryDate.ToString("dd/MM/yyyy"),
            statusLabel = StatusLabel(detail.Status),
            shipperName = detail.ShipperName,
            items = detail.Items.Select(i => new
            {
                orderItemId = i.OrderItemId,
                mealName = i.MealName,
                deliverySlotName = i.DeliverySlotName,
                status = i.Status.ToString(),
                proofImageUrls = i.ProofImageUrls ?? [],
            }),
        });
    }

    // ── Generate delivery orders ───────────────────────────────────────────────
    public async Task<IActionResult> OnPostGenerateAsync()
    {
        var result = await _svc.GenerateDeliveryOrdersForDateAsync(TargetDate);
        SuccessMessage = $"Tạo {result.Generated} đơn ({result.AutoFilled} tự động điền), bỏ qua {result.Skipped}.";
        if (result.Errors > 0)
            ErrorMessage = string.Join("; ", result.ErrorMessages);
        return RedirectToPage(new { TargetDate, StatusFilter });
    }

    // ── Update single order status ─────────────────────────────────────────────
    public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, OrderStatus newStatus)
    {
        try
        {
            await _svc.UpdateStatusAsync(orderId, newStatus);
            SuccessMessage = $"Đã cập nhật đơn #{orderId} → {StatusLabel(newStatus)}.";
            await _orderHub.Clients.Group("Admins").SendAsync("ReceiveOrderStatusUpdate", orderId, newStatus.ToString(), "Admin");
            await _orderHub.Clients.All.SendAsync("ReceiveOrderStatusUpdate", orderId, newStatus.ToString(), "Admin");
            var info = await _svc.GetOrderBroadcastInfoAsync(orderId);
            if (info.HasValue)
            {
                await _trackingHub.Clients.Group("Tracking_Admins").SendAsync("OrderStatusChanged", orderId, newStatus.ToString(), "Admin");
                await _trackingHub.Clients.Group($"Tracking_User_{info.Value.UserId}").SendAsync("OrderStatusChanged", orderId, newStatus.ToString(), "Admin");
                if (info.Value.ShipperId.HasValue)
                    await _trackingHub.Clients.Group($"Tracking_Shipper_{info.Value.ShipperId:D}".ToLowerInvariant()).SendAsync("OrderStatusChanged", orderId, newStatus.ToString(), "Admin");
            }
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        return RedirectToPage(new { TargetDate, StatusFilter });
    }

    // ── Assign Shipper ────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostAssignShipperAsync(int orderId, Guid? shipperId)
    {
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.Any(x => x?.Contains("application/json") == true);
        try
        {
            if (shipperId.HasValue && shipperId.Value != Guid.Empty)
            {
                await _deliveryService.AssignShipperAsync(orderId, shipperId.Value);
                SuccessMessage = $"Đã gán shipper cho đơn #{orderId}.";
                var shippers = await _deliveryService.GetActiveShippersAsync();
                var shipper = shippers.FirstOrDefault(s => s.Id == shipperId.Value);
                var shipperName = shipper?.FullName ?? "Shipper";
                await _orderHub.Clients.Group("Admins").SendAsync("ReceiveShipperAssigned", orderId, shipperName);
                var groupName = $"Shipper_{shipperId.Value:D}".ToLowerInvariant();
                await _orderHub.Clients.Group(groupName).SendAsync("ReceiveNewAssignment", orderId);
            }
            else
            {
                await _deliveryService.UnassignShipperAsync(orderId);
                SuccessMessage = $"Đã bỏ phân công shipper cho đơn #{orderId}.";
            }
            if (isAjax)
                return new JsonResult(new { success = true, message = SuccessMessage });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            if (isAjax)
                return new JsonResult(new { success = false, error = ex.Message });
        }
        return RedirectToPage(new { TargetDate, StatusFilter });
    }

    // ── Bulk update status ─────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostBulkUpdateStatusAsync(List<int> orderIds, OrderStatus bulkStatus)
    {
        if (!orderIds.Any()) { ErrorMessage = "Chưa chọn đơn hàng nào."; return RedirectToPage(new { TargetDate }); }
        var count = await _svc.BulkUpdateStatusAsync(orderIds, bulkStatus);
        SuccessMessage = $"Đã cập nhật {count} đơn → {StatusLabel(bulkStatus)}.";
        foreach (var oid in orderIds)
        {
            await _orderHub.Clients.Group("Admins").SendAsync("ReceiveOrderStatusUpdate", oid, bulkStatus.ToString(), "Admin");
            await _orderHub.Clients.All.SendAsync("ReceiveOrderStatusUpdate", oid, bulkStatus.ToString(), "Admin");
            var info = await _svc.GetOrderBroadcastInfoAsync(oid);
            if (info.HasValue)
            {
                await _trackingHub.Clients.Group("Tracking_Admins").SendAsync("OrderStatusChanged", oid, bulkStatus.ToString(), "Admin");
                await _trackingHub.Clients.Group($"Tracking_User_{info.Value.UserId}").SendAsync("OrderStatusChanged", oid, bulkStatus.ToString(), "Admin");
                if (info.Value.ShipperId.HasValue)
                    await _trackingHub.Clients.Group($"Tracking_Shipper_{info.Value.ShipperId:D}".ToLowerInvariant()).SendAsync("OrderStatusChanged", oid, bulkStatus.ToString(), "Admin");
            }
        }
        return RedirectToPage(new { TargetDate, StatusFilter });
    }

    // ── Download Kitchen CSV ───────────────────────────────────────────────────
    public async Task<IActionResult> OnGetDownloadCsvAsync()
    {
        var items = await _svc.GetKitchenPrepListAsync(TargetDate);
        var sb = new StringBuilder();
        sb.AppendLine("Meal ID,Meal Name,Total Quantity");
        foreach (var item in items)
            sb.AppendLine($"{item.MealId},\"{item.MealName.Replace("\"", "'")}\",{item.TotalQuantity}");
        return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"kitchen-{TargetDate:yyyy-MM-dd}.csv");
    }

    public static string StatusLabel(OrderStatus s) => s switch
    {
        OrderStatus.Planned => "Đã lên kế hoạch",
        OrderStatus.Preparing => "Đang chuẩn bị",
        OrderStatus.Delivering => "Đang giao",
        OrderStatus.Delivered => "Đã giao",
        OrderStatus.ConfirmedByUser => "Đã xác nhận nhận hàng",
        OrderStatus.Cancelled => "Đã hủy",
        _ => s.ToString(),
    };

    public static string StatusBadgeClass(OrderStatus s) => s switch
    {
        OrderStatus.Planned => "bg-gray-100 text-gray-600",
        OrderStatus.Preparing => "bg-amber-100 text-amber-700",
        OrderStatus.Delivering => "bg-blue-100 text-blue-700",
        OrderStatus.Delivered => "bg-emerald-100 text-emerald-700",
        OrderStatus.ConfirmedByUser => "bg-purple-100 text-purple-700",
        OrderStatus.Cancelled => "bg-red-100 text-red-600",
        _ => "bg-gray-100 text-gray-600",
    };

    public static OrderStatus? NextAdminStatus(OrderStatus current) => current switch
    {
        OrderStatus.Planned => OrderStatus.Preparing,
        OrderStatus.Preparing => OrderStatus.Delivering,
        OrderStatus.Delivering => OrderStatus.Delivered,
        _ => null,
    };
}
