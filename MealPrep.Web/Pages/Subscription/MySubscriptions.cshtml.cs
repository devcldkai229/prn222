using System.Security.Claims;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Subscription;

[Authorize]
public class MySubscriptionsModel : PageModel
{
    private readonly ISubscriptionService _subscriptionService;

    public MySubscriptionsModel(ISubscriptionService subscriptionService) =>
        _subscriptionService = subscriptionService;

    public List<UserSubscriptionDto> Subscriptions { get; private set; } = [];

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync()
    {
        Subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(UserId);
    }
}
