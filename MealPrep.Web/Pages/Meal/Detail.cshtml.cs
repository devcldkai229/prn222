using MealPrep.BLL.Services;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace MealPrep.Web.Pages.Meal;

public sealed class MealIngredientDetailVm
{
    public string Name { get; set; } = string.Empty;
    public string QuantityText { get; set; } = "Chưa có định lượng";
    public decimal? QuantityInGrams { get; set; }
}

public sealed class MealReviewItemVm
{
    public string UserName { get; set; } = "Người dùng";
    public int Stars { get; set; }
    public string? Comments { get; set; }
    public string[] Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public class DetailModel : PageModel
{
    private const int DefaultReviewPageSize = 8;

    private readonly IMealService _mealService;
    private readonly AppDbContext _ctx;

    private static readonly Regex QtyPrefixRegex = new(
        @"^\s*(?<qty>\d+(?:[\.,]\d+)?)\s*(?<unit>kg|g|gram|grams)\b\s*(?<name>.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex QtySuffixRegex = new(
        @"^\s*(?<name>.+?)\s*(?<qty>\d+(?:[\.,]\d+)?)\s*(?<unit>kg|g|gram|grams)\b\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public DetailModel(IMealService mealService, AppDbContext ctx)
    {
        _mealService = mealService;
        _ctx = ctx;
    }

    public MealListItemDto Meal { get; private set; } = null!;

    [BindProperty(SupportsGet = true)]
    public int ReviewPage { get; set; } = 1;

    public int ReviewPageSize { get; private set; } = DefaultReviewPageSize;
    public int TotalReviewPages { get; private set; }

    public List<MealIngredientDetailVm> IngredientDetails { get; private set; } = [];
    public List<MealReviewItemVm> Reviews { get; private set; } = [];
    public double AverageStars { get; private set; }
    public int TotalReviews { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var meal = await _mealService.GetByIdAsync(id);
        if (meal == null)
            return NotFound();

        Meal = meal;

        IngredientDetails = (meal.Ingredients ?? [])
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(ParseIngredient)
            .ToList();

        var reviewQuery = _ctx.MealRatings.AsNoTracking().Where(r => r.MealId == id);

        TotalReviews = await reviewQuery.CountAsync();
        AverageStars = TotalReviews > 0
            ? await reviewQuery.AverageAsync(r => (double?)r.Stars) ?? 0
            : 0;

        TotalReviewPages = Math.Max(1, (int)Math.Ceiling(TotalReviews / (double)ReviewPageSize));
        ReviewPage = Math.Clamp(ReviewPage, 1, TotalReviewPages);

        var skip = (ReviewPage - 1) * ReviewPageSize;
        Reviews = await reviewQuery
            .OrderByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(ReviewPageSize)
            .Select(r => new MealReviewItemVm
            {
                UserName = r.User != null ? r.User.FullName : "Người dùng",
                Stars = r.Stars,
                Comments = r.Comments,
                Tags = r.Tags ?? Array.Empty<string>(),
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync();

        return Page();
    }

    private static MealIngredientDetailVm ParseIngredient(string raw)
    {
        var input = raw.Trim();

        var parsed = TryParseIngredient(input, QtyPrefixRegex)
            ?? TryParseIngredient(input, QtySuffixRegex);

        if (parsed != null)
            return parsed;

        return new MealIngredientDetailVm
        {
            Name = input,
            QuantityText = "Chưa có định lượng",
            QuantityInGrams = null,
        };
    }

    private static MealIngredientDetailVm? TryParseIngredient(string input, Regex regex)
    {
        var match = regex.Match(input);
        if (!match.Success)
            return null;

        var qtyRaw = match.Groups["qty"].Value.Replace(',', '.');
        if (!decimal.TryParse(qtyRaw, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var qty))
            return null;

        var unitRaw = match.Groups["unit"].Value.Trim().ToLowerInvariant();
        var name = match.Groups["name"].Value.Trim();

        decimal grams = unitRaw switch
        {
            "kg" => qty * 1000,
            "g" => qty,
            "gram" => qty,
            "grams" => qty,
            _ => qty,
        };

        return new MealIngredientDetailVm
        {
            Name = name,
            QuantityText = $"{qty:0.##} {unitRaw}",
            QuantityInGrams = grams,
        };
    }
}
