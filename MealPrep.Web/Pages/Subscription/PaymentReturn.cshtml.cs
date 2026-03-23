using MealPrep.BLL.Hubs;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace MealPrep.Web.Pages.Subscription;

[AllowAnonymous]
public class PaymentReturnModel : PageModel
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IHubContext<OrderHub> _orderHub;
    private readonly IHubContext<DashboardHub> _dashboardHub;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentReturnModel> _logger;

    public PaymentReturnModel(
        ISubscriptionService subscriptionService,
        IHubContext<OrderHub> orderHub,
        IHubContext<DashboardHub> dashboardHub,
        IConfiguration configuration,
        ILogger<PaymentReturnModel> logger)
    {
        _subscriptionService = subscriptionService;
        _orderHub = orderHub;
        _dashboardHub = dashboardHub;
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsSuccess { get; private set; }
    public string StatusMessage { get; private set; } = "";
    public int? SubscriptionId { get; private set; }
    /// <summary>URL to redirect when user clicks "Xem Gói Đăng Ký" (e.g. localhost:7288/Dashboard).</summary>
    public string RedirectAfterPaymentUrl { get; private set; } = "/Dashboard";

    /// <summary>
    /// MoMo redirects the user here after payment (GET with query params).
    /// returnUrl must be ngrok (public) because MoMo cannot reach localhost.
    /// User sees result; clicking "Xem Gói Đăng Ký" redirects to RedirectAfterPaymentUrl (localhost/Dashboard).
    /// </summary>
    public async Task OnGetAsync()
    {
        RedirectAfterPaymentUrl = _configuration["Momo:RedirectAfterPaymentUrl"]?.Trim() ?? "/Dashboard";

        var resultCode = int.TryParse(Request.Query["resultCode"], out var rc) ? rc : -1;
        var orderId = Request.Query["orderId"].ToString();
        var message = Request.Query["message"].ToString();

        _logger.LogInformation(
            "MoMo payment return – resultCode={ResultCode}, orderId={OrderId}, message={Message}",
            resultCode, orderId, message);

        if (resultCode == 0 && !string.IsNullOrEmpty(orderId))
        {
            try
            {
                var callback = new MomoCallbackDto
                {
                    PartnerCode = Request.Query["partnerCode"].ToString(),
                    OrderId = orderId,
                    RequestId = Request.Query["requestId"].ToString(),
                    Amount = long.TryParse(Request.Query["amount"], out var am) ? am : 0,
                    OrderInfo = Request.Query["orderInfo"].ToString(),
                    OrderType = Request.Query["orderType"].ToString(),
                    TransId = long.TryParse(Request.Query["transId"], out var ti) ? ti : 0,
                    ResultCode = resultCode,
                    Message = message,
                    PayType = Request.Query["payType"].ToString(),
                    ResponseTime = long.TryParse(Request.Query["responseTime"], out var rt) ? rt : 0,
                    ExtraData = Request.Query["extraData"].ToString(),
                    Signature = Request.Query["signature"].ToString(),
                };

                var (success, subId) = await _subscriptionService.ConfirmPaymentByCallbackAsync(callback);
                IsSuccess = success;
                SubscriptionId = subId;
                StatusMessage = IsSuccess
                    ? "Thanh toán thành công! Gói đăng ký của bạn đã được kích hoạt."
                    : "Không tìm thấy thông tin thanh toán. Vui lòng liên hệ hỗ trợ.";
                if (IsSuccess && subId.HasValue)
                {
                    await _orderHub.Clients.Group("Admins").SendAsync("SubscriptionChanged", subId.Value, "Active");
                    await _dashboardHub.Clients.Group("AdminDashboard").SendAsync("ReceiveDashboardUpdate");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment for orderId={OrderId}", orderId);
                IsSuccess = false;
                StatusMessage = "Có lỗi xảy ra khi xử lý thanh toán. Vui lòng liên hệ hỗ trợ.";
            }
        }
        else
        {
            IsSuccess = false;
            StatusMessage = string.IsNullOrEmpty(message)
                ? "Thanh toán không thành công hoặc đã bị hủy."
                : message;
        }
    }
}
