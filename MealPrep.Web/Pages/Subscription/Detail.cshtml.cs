using System.Security.Claims;
using BusinessObjects.Enums;
using MealPrep.BLL.Services;
using MealPrep.BLL.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace MealPrep.Web.Pages.Subscription;

[Authorize]
public class DetailModel : PageModel
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IConfiguration _configuration;
    private readonly IDeliveryProcessingService _deliverySvc;
    private readonly IHubContext<OrderHub> _orderHub;
    private readonly IHubContext<OrderTrackingHub> _trackingHub;
    private readonly IHubContext<DashboardHub> _dashboardHub;

    public DetailModel(
        ISubscriptionService subscriptionService,
        IConfiguration configuration,
        IDeliveryProcessingService deliverySvc,
        IHubContext<OrderHub> orderHub,
        IHubContext<OrderTrackingHub> trackingHub,
        IHubContext<DashboardHub> dashboardHub)
    {
        _subscriptionService = subscriptionService;
        _configuration = configuration;
        _deliverySvc = deliverySvc;
        _orderHub = orderHub;
        _trackingHub = trackingHub;
        _dashboardHub = dashboardHub;
    }

    public SubscriptionDetailDto Detail { get; private set; } = null!;
    public List<UserOrderSummaryDto> RecentOrders { get; private set; } = [];

    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ErrorMessage { get; set; }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var detail = await _subscriptionService.GetSubscriptionDetailsAsync(id, UserId);
        if (detail == null) return NotFound();
        Detail = detail;
        RecentOrders = await _deliverySvc.GetOrdersForSubscriptionAsync(id, UserId);
        return Page();
    }

    public async Task<IActionResult> OnPostConfirmReceiptAsync(int id, int orderId)
    {
        var ok = await _deliverySvc.ConfirmReceiptAsync(orderId, UserId);
        if (ok)
        {
            SuccessMessage = "Đã xác nhận nhận hàng thành công!";
            await _orderHub.Clients.Group("Admins").SendAsync("ReceiveOrderStatusUpdate", orderId, "ConfirmedByUser", "Customer");
            await _orderHub.Clients.All.SendAsync("ReceiveOrderStatusUpdate", orderId, "ConfirmedByUser", "Customer");
            await _trackingHub.Clients.Group("Tracking_Admins").SendAsync("OrderStatusChanged", orderId, "ConfirmedByUser", "Customer");
            await _trackingHub.Clients.Group($"Tracking_User_{UserId}").SendAsync("OrderStatusChanged", orderId, "ConfirmedByUser", "Customer");
            var info = await _deliverySvc.GetOrderBroadcastInfoAsync(orderId);
            if (info.HasValue && info.Value.ShipperId.HasValue)
                await _trackingHub.Clients.Group($"Tracking_Shipper_{info.Value.ShipperId:D}".ToLowerInvariant()).SendAsync("OrderStatusChanged", orderId, "ConfirmedByUser", "Customer");
            await _dashboardHub.Clients.Group("AdminDashboard").SendAsync("ReceiveDashboardUpdate");
        }
        else
            ErrorMessage = "Không thể xác nhận. Đơn hàng phải ở trạng thái 'Đã giao'.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        try
        {
            await _subscriptionService.CancelPendingSubscriptionAsync(id, UserId);
            SuccessMessage = "Đã hủy gói đăng ký thành công.";
            await _orderHub.Clients.Group("Admins").SendAsync("SubscriptionChanged", id, "Cancelled");
            await _dashboardHub.Clients.Group("AdminDashboard").SendAsync("ReceiveDashboardUpdate");
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRetryPaymentAsync(int id)
    {
        try
        {
        var returnBaseUrl = _configuration["Momo:ReturnBaseUrl"]?.Trim()?.TrimEnd('/')
            ?? _configuration["Momo:CallbackBaseUrl"]?.Trim()?.TrimEnd('/')
            ?? $"{Request.Scheme}://{Request.Host}";
        var callbackBaseUrl = _configuration["Momo:CallbackBaseUrl"]?.Trim()?.TrimEnd('/')
            ?? $"{Request.Scheme}://{Request.Host}";
        var returnUrl = $"{returnBaseUrl}/Subscription/PaymentReturn";
        var ipnUrl = $"{callbackBaseUrl}/Subscription/PaymentIpn";

            var result = await _subscriptionService.RetryPaymentAsync(id, UserId, returnUrl, ipnUrl);

            if (!string.IsNullOrEmpty(result.MomoRedirectUrl))
                return Redirect(result.MomoRedirectUrl);

            ErrorMessage = "Không thể tạo link thanh toán. Vui lòng thử lại.";
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage(new { id });
    }

    // ── Flow 7: Pause ──────────────────────────────────────────────────────────
    [BindProperty] public DateOnly PauseFrom { get; set; }
    [BindProperty] public DateOnly PauseTo { get; set; }

    public async Task<IActionResult> OnPostPauseAsync(int id)
    {
        try
        {
            await _subscriptionService.PauseSubscriptionAsync(id, UserId, PauseFrom, PauseTo);
            SuccessMessage = $"Đã tạm ngưng gói từ {PauseFrom:dd/MM/yyyy} đến {PauseTo:dd/MM/yyyy}. Ngày kết thúc được kéo dài tương ứng.";
        }
        catch (InvalidOperationException ex) { ErrorMessage = ex.Message; }
        return RedirectToPage(new { id });
    }

    // ── Flow 7: Resume ─────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostResumeAsync(int id)
    {
        try
        {
            await _subscriptionService.ResumeSubscriptionAsync(id, UserId);
            SuccessMessage = "Đã tiếp tục gói đăng ký thành công.";
        }
        catch (InvalidOperationException ex) { ErrorMessage = ex.Message; }
        return RedirectToPage(new { id });
    }

    // ── Flow 7: Cancel Active ──────────────────────────────────────────────────
    public async Task<IActionResult> OnPostCancelActiveAsync(int id)
    {
        try
        {
            await _subscriptionService.CancelActiveSubscriptionAsync(id, UserId);
            SuccessMessage = "Đã hủy gói. Các đơn giao hàng chưa được giao sẽ bị hủy.";
            await _orderHub.Clients.Group("Admins").SendAsync("SubscriptionChanged", id, "Cancelled");
            await _dashboardHub.Clients.Group("AdminDashboard").SendAsync("ReceiveDashboardUpdate");
        }
        catch (InvalidOperationException ex) { ErrorMessage = ex.Message; }
        return RedirectToPage(new { id });
    }

    // ── Flow 7: Renew (redirect to Plans to pick and checkout again) ───────────
    public IActionResult OnPostRenew(int id)
    {
        return RedirectToPage("/Subscription/Plans");
    }
}
