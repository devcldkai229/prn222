using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class SupportModel : PageModel
{
    private readonly IAdminSupportService _supportService;

    public SupportModel(IAdminSupportService supportService) => _supportService = supportService;

    public List<DisputedOrderDto> DisputedOrders { get; private set; } = [];

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        DisputedOrders = await _supportService.GetDisputedOrdersAsync();
    }

    public async Task<IActionResult> OnGetDisputedOrderAsync(int orderId)
    {
        var order = await _supportService.GetDisputedOrderAsync(orderId);
        if (order == null) return new JsonResult(new { found = false });
        return new JsonResult(new { found = true, order });
    }

    public async Task<IActionResult> OnPostRescheduleAsync(int orderId, DateOnly newDate)
    {
        try
        {
            var newOrderId = await _supportService.RescheduleMissingOrderAsync(orderId, newDate);
            return new JsonResult(
                new
                {
                    success = true,
                    newOrderId,
                    message = $"Replacement order #{newOrderId} created for {newDate:MMM dd, yyyy}.",
                }
            );
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }
}
