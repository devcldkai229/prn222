using System.Text.Json;
using BusinessObjects.Entities;
using MealPrep.BLL.Services.Interfaces;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class MealListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
    public bool IsActive { get; set; }
    public double? AvgRating { get; set; }
    public string[] Ingredients { get; set; } = [];
    public string[] Images { get; set; } = [];
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class CreateMealDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
    public string IngredientsRaw { get; set; } = ""; // comma-separated from frontend
    public bool IsActive { get; set; } = true;
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IMealService
{
    Task<PagedResult<MealListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null
    );
    Task<MealListItemDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(
        CreateMealDto dto,
        IEnumerable<(Stream Stream, string FileName, string ContentType)> images
    );
    Task UpdateAsync(
        int id,
        CreateMealDto dto,
        IEnumerable<(Stream Stream, string FileName, string ContentType)>? newImages
    );
    Task SoftDeleteAsync(int id);
    Task<List<MealListItemDto>> GetFeaturedAsync(int count = 6);
    Task<PagedResult<MealListItemDto>> SearchWithPaginationAsync(
        string? keyword,
        int page,
        int pageSize
    );
}

// ── Implementation ────────────────────────────────────────────────────────────

public class MealService : IMealService
{
    private readonly AppDbContext _ctx;
    private readonly IS3Service _s3;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<MealService> _logger;

    public MealService(
        AppDbContext ctx,
        IS3Service s3,
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<MealService> logger)
    {
        _ctx = ctx;
        _s3 = s3;
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<PagedResult<MealListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null
    )
    {
        var q = _ctx.Meals.AsQueryable();

        if (isActive.HasValue)
            q = q.Where(m => m.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            q = q.Where(m =>
                m.Name.ToLower().Contains(lower)
                || (m.Description != null && m.Description.ToLower().Contains(lower))
            );
        }

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Avg ratings (batch)
        var ids = items.Select(m => m.Id).ToList();
        var avgRatings = await _ctx
            .MealRatings.Where(r => ids.Contains(r.MealId))
            .GroupBy(r => r.MealId)
            .Select(g => new { g.Key, Avg = (double?)g.Average(r => r.Stars) })
            .ToDictionaryAsync(x => x.Key, x => x.Avg);

        return new PagedResult<MealListItemDto>
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Items = items.Select(m => ToListDto(m, avgRatings.GetValueOrDefault(m.Id), _s3)).ToList(),
        };
    }

    public async Task<MealListItemDto?> GetByIdAsync(int id)
    {
        var m = await _ctx.Meals.FindAsync(id);
        if (m == null)
            return null;
        return ToListDto(m, null, _s3);
    }

    public async Task<int> CreateAsync(
        CreateMealDto dto,
        IEnumerable<(Stream Stream, string FileName, string ContentType)> images
    )
    {
        var imageKeys = await UploadImagesAsync(images);

        var meal = new Meal
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Calories = dto.Calories,
            Protein = dto.Protein,
            Carbs = dto.Carbs,
            Fat = dto.Fat,
            Ingredients = ParseIngredients(dto.IngredientsRaw),
            Images = imageKeys,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
        };

        _ctx.Meals.Add(meal);
        await _ctx.SaveChangesAsync();

        await GenerateAndSaveEmbeddingAsync(meal.Id, meal.Name, meal.Ingredients, meal.Description ?? "", meal.Calories, meal.Protein, meal.Carbs, meal.Fat);

        return meal.Id;
    }

    public async Task UpdateAsync(
        int id,
        CreateMealDto dto,
        IEnumerable<(Stream Stream, string FileName, string ContentType)>? newImages
    )
    {
        var meal =
            await _ctx.Meals.FindAsync(id)
            ?? throw new InvalidOperationException("Meal not found.");

        meal.Name = dto.Name.Trim();
        meal.Description = dto.Description?.Trim();
        meal.Calories = dto.Calories;
        meal.Protein = dto.Protein;
        meal.Carbs = dto.Carbs;
        meal.Fat = dto.Fat;
        meal.Ingredients = ParseIngredients(dto.IngredientsRaw);
        meal.IsActive = dto.IsActive;
        meal.UpdatedAt = DateTime.UtcNow;

        if (newImages != null)
        {
            var newKeys = await UploadImagesAsync(newImages);
            meal.Images = [.. meal.Images, .. newKeys];
        }

        await _ctx.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var meal =
            await _ctx.Meals.FindAsync(id)
            ?? throw new InvalidOperationException("Meal not found.");
        meal.IsActive = false;
        meal.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }

    public async Task<List<MealListItemDto>> GetFeaturedAsync(int count = 6)
    {
        // Get top-rated active meals
        var topMealIds = await _ctx
            .MealRatings.GroupBy(r => r.MealId)
            .OrderByDescending(g => g.Average(r => r.Stars))
            .Select(g => g.Key)
            .Take(count)
            .ToListAsync();

        var meals = await _ctx
            .Meals.Where(m => m.IsActive && topMealIds.Contains(m.Id))
            .ToListAsync();

        // Fill up to count with newest meals if not enough ratings
        if (meals.Count < count)
        {
            var exclude = meals.Select(m => m.Id).ToList();
            var extra = await _ctx
                .Meals.Where(m => m.IsActive && !exclude.Contains(m.Id))
                .OrderByDescending(m => m.CreatedAt)
                .Take(count - meals.Count)
                .ToListAsync();
            meals.AddRange(extra);
        }

        var ids = meals.Select(m => m.Id).ToList();
        var avgRatings = await _ctx
            .MealRatings.Where(r => ids.Contains(r.MealId))
            .GroupBy(r => r.MealId)
            .Select(g => new { g.Key, Avg = (double?)g.Average(r => r.Stars) })
            .ToDictionaryAsync(x => x.Key, x => x.Avg);

        return meals.Select(m => ToListDto(m, avgRatings.GetValueOrDefault(m.Id), _s3)).ToList();
    }

    public Task<PagedResult<MealListItemDto>> SearchWithPaginationAsync(
        string? keyword,
        int page,
        int pageSize
    ) => GetPagedAsync(page, pageSize, keyword, isActive: true);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string[]> UploadImagesAsync(
        IEnumerable<(Stream Stream, string FileName, string ContentType)> images
    )
    {
        var keys = new List<string>();
        foreach (var (stream, fileName, contentType) in images)
        {
            var key = await _s3.UploadFileAsync(stream, fileName, "meals", contentType);
            keys.Add(key);
        }
        return [.. keys];
    }


    private async Task GenerateAndSaveEmbeddingAsync(int mealId, string name, string[] ingredients, string description, int calories, decimal protein, decimal carbs, decimal fat)
    {
        var baseUrl = (_config["RecommendationAi:BaseUrl"] ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
        {
            _logger.LogWarning("RecommendationAi:BaseUrl not configured. Skipping embedding generation for meal {MealId}.", mealId);
            return;
        }

        try
        {
            var payload = new
            {
                meal_id = mealId,
                name,
                ingredients = JsonSerializer.Serialize(ingredients),
                description,
                calories,
                protein = (float)protein,
                carbs = (float)carbs,
                fat = (float)fat,
            };

            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            using var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{baseUrl}/api/generate-meal-embedding", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Embedding API returned {StatusCode} for meal {MealId}: {Content}",
                    response.StatusCode, mealId, await response.Content.ReadAsStringAsync());
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.TryGetProperty("embedding_json", out var embProp))
            {
                _logger.LogWarning("Embedding API response missing embedding_json for meal {MealId}.", mealId);
                return;
            }

            var embeddingList = JsonSerializer.Deserialize<double[]>(embProp.GetString() ?? "[]");
            if (embeddingList == null || embeddingList.Length == 0)
            {
                _logger.LogWarning("Embedding API returned empty embedding for meal {MealId}.", mealId);
                return;
            }

            var meal = await _ctx.Meals.FindAsync(mealId);
            if (meal != null)
            {
                meal.Embedding = embeddingList;
                meal.UpdatedAt = DateTime.UtcNow;
                await _ctx.SaveChangesAsync();
                _logger.LogInformation("Successfully generated and saved embedding for meal {MealId}.", mealId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate embedding for meal {MealId}. Embedding can be generated later via batch API.", mealId);
        }
    }

    private static string[] ParseIngredients(string raw) =>
        raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static MealListItemDto ToListDto(Meal m, double? avgRating, IS3Service s3)
    {
        var imageUrls = s3.ResolveMealImageUrls(m.Images ?? []);
        return new MealListItemDto
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description,
            ThumbnailUrl = imageUrls.FirstOrDefault(),
            Calories = m.Calories,
            Protein = m.Protein,
            Carbs = m.Carbs,
            Fat = m.Fat,
            IsActive = m.IsActive,
            AvgRating = avgRating,
            Ingredients = m.Ingredients,
            Images = imageUrls,
        };
    }
}
