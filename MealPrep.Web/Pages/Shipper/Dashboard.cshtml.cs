using System.Security.Claims;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Shipper;

[Authorize(Roles = "Shipper,Admin")]
public class DashboardModel : PageModel
{
    private readonly IShipperService _shipperService;

    public DashboardModel(IShipperService shipperService) =>
        _shipperService = shipperService;

    public List<ShipperOrderDto> TodaysOrders { get; private set; } = [];
    public int TotalOrders => TodaysOrders.Count;
    public int CompletedOrders => TodaysOrders.Count(o => o.Status == BusinessObjects.Enums.OrderStatus.Completed);
    public int PendingOrders => TotalOrders - CompletedOrders;
    public int TotalItems => TodaysOrders.Sum(o => o.Items.Count);
    public int DeliveredItems => TodaysOrders.Sum(o => o.Items.Count(i => i.IsDelivered));

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync()
    {
        TodaysOrders = await _shipperService.GetTodaysOrdersAsync(UserId);
    }
}
