using BusinessObjects.Enums;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class SubscriptionsModel : PageModel
{
    private readonly IAdminSubscriptionService _subService;

    public SubscriptionsModel(IAdminSubscriptionService subService) => _subService = subService;

    public PagedResult<SubscriptionSummaryDto> Result { get; private set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public SubscriptionStatus? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;
    public const int PageSize = 15;

    [TempData]
    public string? SuccessMessage { get; set; }

    public IEnumerable<SubscriptionStatus> AllStatuses => Enum.GetValues<SubscriptionStatus>();

    public async Task OnGetAsync()
    {
        Result = await _subService.GetPagedAsync(StatusFilter, Search, PageIndex, PageSize);
    }

    public async Task<IActionResult> OnPostOverrideStatusAsync(
        int subId,
        SubscriptionStatus newStatus
    )
    {
        await _subService.OverrideStatusAsync(subId, newStatus);
        SuccessMessage = $"Subscription #{subId} status changed to {newStatus}.";
        return RedirectToPage();
    }
}
