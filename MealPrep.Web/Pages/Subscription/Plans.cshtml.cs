using System.Security.Claims;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Subscription;

[Authorize]
public class PlansModel : PageModel
{
    private readonly ISubscriptionService _subscriptionService;

    public PlansModel(ISubscriptionService subscriptionService) =>
        _subscriptionService = subscriptionService;

    public List<PlanDto> Plans { get; private set; } = [];

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync()
    {
        Plans = (await _subscriptionService.GetActivePlansAsync())
            .Where(p => p.DurationDays != 30)
            .ToList();
    }
}
