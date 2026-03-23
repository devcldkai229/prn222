using System.Net.Http.Headers;
using System.Text.Json;
using MealPrep.BLL.Hubs;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace MealPrep.Web.Pages.Admin.Meals;

[Authorize(Roles = "Admin")]
public class CreateEditModel : PageModel
{
    private readonly IMealService _mealService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<MealHub> _mealHub;

    public CreateEditModel(
        IMealService mealService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHubContext<MealHub> mealHub)
    {
        _mealService = mealService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _mealHub = mealHub;
    }

    /// <summary>URL của voice_generation_ai (port 8001). Dùng cho nút "Voice Input".</summary>
    public string VoiceAiBaseUrl => _configuration["GenerateMealAi:BaseUrl"] ?? "http://localhost:8001";

    [BindProperty]
    public CreateMealDto Dto { get; set; } = new();

    [BindProperty]
    public int? MealId { get; set; }

    [BindProperty]
    public List<IFormFile> Images { get; set; } = [];

    public bool IsEdit => MealId.HasValue;
    public MealListItemDto? ExistingMeal { get; private set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(int? mealId)
    {
        MealId = mealId;
        if (mealId.HasValue)
        {
            ExistingMeal = await _mealService.GetByIdAsync(mealId.Value);
            if (ExistingMeal != null)
            {
                Dto = new CreateMealDto
                {
                    Name = ExistingMeal.Name,
                    Description = ExistingMeal.Description,
                    Calories = ExistingMeal.Calories,
                    Protein = ExistingMeal.Protein,
                    Carbs = ExistingMeal.Carbs,
                    Fat = ExistingMeal.Fat,
                    IngredientsRaw = string.Join(", ", ExistingMeal.Ingredients),
                    IsActive = ExistingMeal.IsActive,
                };
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var imageStreams = Images
            .Where(f => f.Length > 0)
            .Select(f => (f.OpenReadStream(), f.FileName, f.ContentType))
            .ToList();

        try
        {
            int mealId;
            if (MealId.HasValue)
            {
                await _mealService.UpdateAsync(
                    MealId.Value,
                    Dto,
                    imageStreams.Any() ? imageStreams : null
                );
                mealId = MealId.Value;
                TempData["SuccessMessage"] = "Meal updated successfully.";

                var meal = await _mealService.GetByIdAsync(mealId);
                if (meal != null)
                    await _mealHub.Clients.All.SendAsync("MealUpdated", new
                    {
                        id = meal.Id,
                        name = meal.Name,
                        description = meal.Description,
                        calories = meal.Calories,
                        protein = meal.Protein,
                        carbs = meal.Carbs,
                        fat = meal.Fat,
                        thumbnailUrl = meal.ThumbnailUrl,
                        images = meal.Images ?? Array.Empty<string>(),
                        ingredients = meal.Ingredients ?? Array.Empty<string>(),
                        isActive = meal.IsActive,
                        avgRating = meal.AvgRating,
                    });
            }
            else
            {
                mealId = await _mealService.CreateAsync(Dto, imageStreams);
                TempData["SuccessMessage"] = "Meal created successfully.";

                var meal = await _mealService.GetByIdAsync(mealId);
                if (meal != null)
                    await _mealHub.Clients.All.SendAsync("MealCreated", new
                    {
                        id = meal.Id,
                        name = meal.Name,
                        description = meal.Description,
                        calories = meal.Calories,
                        protein = meal.Protein,
                        carbs = meal.Carbs,
                        fat = meal.Fat,
                        thumbnailUrl = meal.ThumbnailUrl,
                        images = meal.Images ?? Array.Empty<string>(),
                        ingredients = meal.Ingredients ?? Array.Empty<string>(),
                        isActive = meal.IsActive,
                        avgRating = meal.AvgRating,
                    });
            }
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
        finally
        {
            // Dispose all streams
            foreach (var (stream, _, _) in imageStreams)
                await stream.DisposeAsync();
        }
    }

    /// <summary>
    /// Nhận audio từ admin, gửi đến voice_generation_ai (AWS Transcribe + Bedrock),
    /// trả về JSON để fill form Thêm món ăn.
    /// </summary>
    public async Task<IActionResult> OnPostAnalyzeVoiceAsync(IFormFile? audioFile)
    {
        if (audioFile == null || audioFile.Length == 0)
            return new JsonResult(new { success = false, error = "Chưa chọn file audio." });

        var baseUrl = (VoiceAiBaseUrl ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
            return new JsonResult(new { success = false, error = "Voice AI chưa được cấu hình (GenerateMealAi:BaseUrl)." });

        try
        {
            using var content = new MultipartFormDataContent();
            using var stream = audioFile.OpenReadStream();
            var streamContent = new StreamContent(stream);
            var safeContentType = audioFile.ContentType?.Split(';')[0].Trim() ?? "audio/webm";
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(safeContentType);
            content.Add(streamContent, "file", audioFile.FileName);

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(2);

            var response = await client.PostAsync($"{baseUrl}/api/analyze-voice", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new JsonResult(new { success = false, error = $"Voice AI lỗi: {json}" });

            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var name = root.TryGetProperty("dish_name", out var dn) ? dn.GetString() ?? "" : "";
            var description = root.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "";

            int calories = 0;
            decimal protein = 0, carbs = 0, fat = 0;
            if (root.TryGetProperty("total_nutrition", out var tn))
            {
                calories = tn.TryGetProperty("calories", out var c) ? c.GetInt32() : 0;
                protein = tn.TryGetProperty("protein_g", out var p) ? p.GetDecimal() : 0;
                carbs = tn.TryGetProperty("carbs_g", out var cb) ? cb.GetDecimal() : 0;
                fat = tn.TryGetProperty("fat_g", out var f) ? f.GetDecimal() : 0;
            }

            var ingredientsRaw = "";
            // Ưu tiên dùng chuỗi đã format sẵn (có số lượng) nếu API trả về
            if (root.TryGetProperty("ingredients_raw", out var ir) && ir.ValueKind == JsonValueKind.String)
            {
                ingredientsRaw = ir.GetString() ?? "";
            }
            else if (root.TryGetProperty("ingredients_text", out var it) && it.ValueKind == JsonValueKind.String)
            {
                ingredientsRaw = it.GetString() ?? "";
            }
            else if (root.TryGetProperty("ingredients", out var ing) && ing.ValueKind == JsonValueKind.Array)
            {
                var parts = new List<string>();
                foreach (var item in ing.EnumerateArray())
                {
                    var ingName = item.TryGetProperty("name", out var n) ? (n.GetString() ?? "").Trim() : "";
                    if (string.IsNullOrWhiteSpace(ingName)) continue;

                    // Nếu có quantity và unit thì ghép: "200g ức gà", "15ml mật ong"
                    string? qtyStr = null;
                    if (item.TryGetProperty("quantity", out var q))
                    {
                        if (q.ValueKind == JsonValueKind.Number && q.TryGetDecimal(out var qty))
                            qtyStr = qty % 1 == 0 ? ((int)qty).ToString() : qty.ToString("0.##");
                        else if (q.ValueKind == JsonValueKind.String)
                            qtyStr = q.GetString() ?? "";
                    }
                    var unit = item.TryGetProperty("unit", out var u) ? (u.GetString() ?? "").Trim() : "";
                    if (!string.IsNullOrEmpty(qtyStr) && !string.IsNullOrEmpty(unit))
                        parts.Add($"{qtyStr}{unit} {ingName}");
                    else if (!string.IsNullOrEmpty(qtyStr))
                        parts.Add($"{qtyStr} {ingName}");
                    else
                        parts.Add(ingName);
                }
                ingredientsRaw = string.Join(", ", parts);
            }

            return new JsonResult(new
            {
                success = true,
                name,
                description,
                calories,
                protein,
                carbs,
                fat,
                ingredientsRaw,
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }
}
