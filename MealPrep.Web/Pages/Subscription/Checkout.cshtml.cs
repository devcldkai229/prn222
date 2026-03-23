using System.Security.Claims;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Subscription;

[Authorize]
public class CheckoutModel : PageModel
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IConfiguration _configuration;

    public CheckoutModel(ISubscriptionService subscriptionService, IConfiguration configuration)
    {
        _subscriptionService = subscriptionService;
        _configuration = configuration;
    }

    [BindProperty]
    public int PlanId { get; set; }

    [BindProperty]
    public DateOnly StartDate { get; set; }

    [BindProperty]
    public int MealsPerDay { get; set; } = 2;

    [BindProperty]
    public string DeliveryTimeSlot { get; set; } = "Morning";

    public PlanDto? SelectedPlan { get; private set; }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync(int planId)
    {
        PlanId = planId;
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var plans = await _subscriptionService.GetActivePlansAsync();
        SelectedPlan = plans.FirstOrDefault(p => p.Id == planId);

        if (SelectedPlan != null)
            MealsPerDay = SelectedPlan.MealsPerDay;
    }

    public async Task<IActionResult> OnPostCheckoutAsync()
    {
        // returnUrl: user redirect sau thanh toán (có thể localhost khi dev)
        // ipnUrl: server-to-server callback từ MoMo (bắt buộc public/ngrok)
        var returnBaseUrl = _configuration["Momo:ReturnBaseUrl"]?.Trim()?.TrimEnd('/')
            ?? _configuration["Momo:CallbackBaseUrl"]?.Trim()?.TrimEnd('/')
            ?? $"{Request.Scheme}://{Request.Host}";
        var callbackBaseUrl = _configuration["Momo:CallbackBaseUrl"]?.Trim()?.TrimEnd('/')
            ?? $"{Request.Scheme}://{Request.Host}";
        var returnUrl = $"{returnBaseUrl}/Subscription/PaymentReturn";
        var ipnUrl = $"{callbackBaseUrl}/Subscription/PaymentIpn";

        var dto = new CreateSubscriptionDto
        {
            PlanId = PlanId,
            StartDate = StartDate,
            MealsPerDay = MealsPerDay,
            DeliveryTimeSlot = DeliveryTimeSlot,
        };

        var result = await _subscriptionService.CreateSubscriptionWithPaymentAsync(
            UserId,
            dto,
            returnUrl,
            ipnUrl
        );
        return Redirect(result.MomoRedirectUrl);
    }
}
