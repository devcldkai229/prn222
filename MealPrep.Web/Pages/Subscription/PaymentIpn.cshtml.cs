using System.Text.Json;
using MealPrep.BLL.Hubs;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace MealPrep.Web.Pages.Subscription;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class PaymentIpnModel : PageModel
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IHubContext<OrderHub> _orderHub;
    private readonly IHubContext<DashboardHub> _dashboardHub;
    private readonly ILogger<PaymentIpnModel> _logger;

    public PaymentIpnModel(
        ISubscriptionService subscriptionService,
        IHubContext<OrderHub> orderHub,
        IHubContext<DashboardHub> dashboardHub,
        ILogger<PaymentIpnModel> logger)
    {
        _subscriptionService = subscriptionService;
        _orderHub = orderHub;
        _dashboardHub = dashboardHub;
        _logger = logger;
    }

    /// <summary>
    /// MoMo IPN (Instant Payment Notification) – server-to-server POST callback.
    /// Must always return 200 OK so MoMo does not keep retrying.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("MoMo IPN callback received");

        try
        {
            Request.EnableBuffering();
            Request.Body.Position = 0;

            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            _logger.LogInformation("IPN Body: {Body}", body);

            var root = JsonDocument.Parse(body).RootElement;

            var callback = new MomoCallbackDto
            {
                PartnerCode = root.TryGetProperty("partnerCode", out var pc) ? pc.GetString() ?? "" : "",
                OrderId     = root.TryGetProperty("orderId",     out var oi) ? oi.GetString() ?? "" : "",
                RequestId   = root.TryGetProperty("requestId",   out var ri) ? ri.GetString() ?? "" : "",
                Amount      = root.TryGetProperty("amount",      out var am) ? am.GetInt64()        : 0,
                OrderInfo   = root.TryGetProperty("orderInfo",   out var info) ? info.GetString() ?? "" : "",
                OrderType   = root.TryGetProperty("orderType",   out var ot) ? ot.GetString() ?? "" : "",
                TransId     = root.TryGetProperty("transId",     out var ti) ? ti.GetInt64()        : 0,
                ResultCode  = root.TryGetProperty("resultCode",  out var rc) ? rc.GetInt32()        : -1,
                Message     = root.TryGetProperty("message",     out var msg) ? msg.GetString() ?? "" : "",
                PayType     = root.TryGetProperty("payType",     out var pt) ? pt.GetString() ?? "" : "",
                ResponseTime = root.TryGetProperty("responseTime", out var rt) ? rt.GetInt64()      : 0,
                ExtraData   = root.TryGetProperty("extraData",   out var ed) ? ed.GetString() ?? "" : "",
                Signature   = root.TryGetProperty("signature",   out var sig) ? sig.GetString() ?? "" : "",
            };

            _logger.LogInformation(
                "IPN parsed – resultCode={ResultCode}, orderId={OrderId}, amount={Amount}",
                callback.ResultCode, callback.OrderId, callback.Amount
            );

            var (ok, subId) = await _subscriptionService.HandleMomoCallbackAsync(callback);
            _logger.LogInformation("IPN result: {Ok}, subId: {SubId}", ok, subId);
            if (ok && subId.HasValue)
            {
                await _orderHub.Clients.Group("Admins").SendAsync("SubscriptionChanged", subId.Value, "Active");
                await _dashboardHub.Clients.Group("AdminDashboard").SendAsync("ReceiveDashboardUpdate");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IPN: error processing callback");
        }

        // Always return 200 OK to MoMo
        return new JsonResult(new { resultCode = 0, message = "Success" });
    }
}
