using System.Security.Claims;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Dashboard;

[Authorize]
public class OrderDetailModel : PageModel
{
    private readonly IOrderTrackingService _trackingService;

    public OrderDetailModel(IOrderTrackingService trackingService) => _trackingService = trackingService;

    public UserOrderDto? Order { get; set; }
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync(int? id)
    {
        if (id.HasValue)
            Order = await _trackingService.GetOrderDetailAsync(id.Value, UserId);
    }
}
