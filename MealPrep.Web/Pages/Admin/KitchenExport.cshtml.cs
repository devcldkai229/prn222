using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace MealPrep.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class KitchenExportModel : PageModel
{
    private readonly IDeliveryProcessingService _deliveryService;

    public KitchenExportModel(IDeliveryProcessingService deliveryService) =>
        _deliveryService = deliveryService;

    public List<KitchenPrepItem> KitchenList { get; private set; } = [];
    public DateOnly SelectedDate { get; private set; }

    public async Task OnGetAsync(DateOnly? date)
    {
        SelectedDate = date ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        KitchenList = await _deliveryService.GetKitchenPrepListAsync(SelectedDate);
    }

    public async Task<IActionResult> OnGetDownloadCsvAsync(DateOnly date)
    {
        var items = await _deliveryService.GetKitchenPrepListAsync(date);
        var csv = new StringBuilder();
        csv.AppendLine("Meal ID,Meal Name,Total Quantity");
        foreach (var item in items)
        {
            csv.AppendLine($"{item.MealId},\"{item.MealName}\",{item.TotalQuantity}");
        }
        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"KitchenList_{date:yyyyMMdd}.csv");
    }
}
