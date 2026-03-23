using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.BLL.Services.Interfaces;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class PendingFeedbackDto
{
    public int OrderItemId { get; set; }
    public int MealId { get; set; }
    public string MealName { get; set; } = "";
    public string[] ImageUrls { get; set; } = [];
    public DateOnly DeliveryDate { get; set; }
    public int OrderId { get; set; }
}

public class SubmitRatingDto
{
    public int OrderItemId { get; set; }
    public int MealId { get; set; }
    public int Stars { get; set; }
    public string[] Tags { get; set; } = [];
    public string? Comments { get; set; }
    public DateOnly DeliveryDate { get; set; }
}

public class UserFeedbackSummaryDto
{
    public int TotalRatings { get; set; }
    public double AverageStars { get; set; }
    public Dictionary<int, int> StarDistribution { get; set; } = new();
    public List<TopRatedMealDto> TopRatedMeals { get; set; } = [];
    public List<TopRatedMealDto> LowestRatedMeals { get; set; } = [];
}

public class TopRatedMealDto
{
    public int MealId { get; set; }
    public string MealName { get; set; } = "";
    public double AverageStars { get; set; }
    public int RatingCount { get; set; }
}

public class MealFeedbackReportDto
{
    public int MealId { get; set; }
    public string MealName { get; set; } = "";
    public string[] ImageUrls { get; set; } = [];
    public double AverageStars { get; set; }
    public int TotalRatings { get; set; }
    public int OneStarCount { get; set; }
    public int TwoStarCount { get; set; }
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IMealFeedbackService
{
    Task<List<PendingFeedbackDto>> GetPendingFeedbacksAsync(Guid userId);
    Task SubmitRatingAsync(Guid userId, SubmitRatingDto dto);
    Task<double> GetMealAvgRatingAsync(int mealId);
    Task<UserFeedbackSummaryDto> GetUserFeedbackSummaryAsync(Guid userId);
    Task<List<MealFeedbackReportDto>> GetLowRatedMealsReportAsync(int minRatings = 3);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class MealFeedbackService : IMealFeedbackService
{
    private readonly AppDbContext _ctx;
    private readonly IUserService _userService;
    private readonly IS3Service _s3;

    public MealFeedbackService(AppDbContext ctx, IUserService userService, IS3Service s3)
    {
        _ctx = ctx;
        _userService = userService;
        _s3 = s3;
    }

    public async Task<List<PendingFeedbackDto>> GetPendingFeedbacksAsync(Guid userId)
    {
        // Get all delivered OrderItems for this user
        var deliveredItems = await _ctx
            .OrderItems.Include(oi => oi.Order)
            .Include(oi => oi.Meal)
            .Where(oi => oi.Order!.UserId == userId && oi.Status == OrderItemStatus.Delivered)
            .ToListAsync();

        // Filter out items that already have a rating
        var ratedOrderItemIds = await _ctx
            .MealRatings.Where(r => r.UserId == userId)
            .Select(r => r.OrderItemId)
            .ToListAsync();

        return deliveredItems
            .Where(oi => !ratedOrderItemIds.Contains(oi.Id))
            .Select(oi => new PendingFeedbackDto
            {
                OrderItemId = oi.Id,
                MealId = oi.MealId,
                MealName = oi.Meal?.Name ?? "",
                ImageUrls = _s3.ResolveMealImageUrls(oi.Meal?.Images ?? []),
                DeliveryDate = oi.Order!.DeliveryDate,
                OrderId = oi.OrderId,
            })
            .OrderByDescending(x => x.DeliveryDate)
            .ToList();
    }

    public async Task SubmitRatingAsync(Guid userId, SubmitRatingDto dto)
    {
        // Validate OrderItem belongs to user
        var orderItem =
            await _ctx
                .OrderItems.Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.Id == dto.OrderItemId && oi.Order!.UserId == userId)
            ?? throw new InvalidOperationException("Order item not found or access denied.");

        // Prevent duplicate rating
        if (
            await _ctx.MealRatings.AnyAsync(r =>
                r.OrderItemId == dto.OrderItemId && r.UserId == userId
            )
        )
            throw new InvalidOperationException("This meal has already been rated.");

        var rating = new MealRating
        {
            UserId = userId,
            OrderItemId = dto.OrderItemId,
            MealId = dto.MealId,
            DeliveryDate = dto.DeliveryDate,
            Stars = dto.Stars,
            Tags = dto.Tags,
            Comments = dto.Comments,
            CreatedAt = DateTime.UtcNow,
        };

        _ctx.MealRatings.Add(rating);
        await _ctx.SaveChangesAsync();

        // Auto-dislike rule: Stars <= 2 → add to DislikedMeals
        if (dto.Stars <= 2)
        {
            await _userService.AddDislikedMealAsync(userId, dto.MealId);
        }
    }

    public async Task<double> GetMealAvgRatingAsync(int mealId)
    {
        var avg = await _ctx.MealRatings
            .Where(r => r.MealId == mealId)
            .AverageAsync(r => (double?)r.Stars);
        return avg ?? 0;
    }

    public async Task<UserFeedbackSummaryDto> GetUserFeedbackSummaryAsync(Guid userId)
    {
        var ratings = await _ctx.MealRatings
            .Include(r => r.Meal)
            .Where(r => r.UserId == userId)
            .ToListAsync();

        if (!ratings.Any())
            return new UserFeedbackSummaryDto();

        var starDist = Enumerable.Range(1, 5)
            .ToDictionary(s => s, s => ratings.Count(r => r.Stars == s));

        var byMeal = ratings.GroupBy(r => new { r.MealId, r.Meal!.Name })
            .Select(g => new TopRatedMealDto
            {
                MealId = g.Key.MealId,
                MealName = g.Key.Name,
                AverageStars = g.Average(r => r.Stars),
                RatingCount = g.Count(),
            }).ToList();

        return new UserFeedbackSummaryDto
        {
            TotalRatings = ratings.Count,
            AverageStars = ratings.Average(r => r.Stars),
            StarDistribution = starDist,
            TopRatedMeals = byMeal.OrderByDescending(m => m.AverageStars).Take(5).ToList(),
            LowestRatedMeals = byMeal.OrderBy(m => m.AverageStars).Take(5).ToList(),
        };
    }

    public async Task<List<MealFeedbackReportDto>> GetLowRatedMealsReportAsync(int minRatings = 3)
    {
        var raw = await _ctx.MealRatings
            .Include(r => r.Meal)
            .GroupBy(r => new { r.MealId, r.Meal!.Name, Images = r.Meal!.Images })
            .Where(g => g.Count() >= minRatings)
            .Select(g => new { g.Key.MealId, g.Key.Name, g.Key.Images, Avg = g.Average(r => r.Stars), Total = g.Count(), One = g.Count(r => r.Stars == 1), Two = g.Count(r => r.Stars == 2) })
            .OrderBy(x => x.Avg)
            .ToListAsync();
        return raw.Select(x => new MealFeedbackReportDto
        {
            MealId = x.MealId,
            MealName = x.Name,
            ImageUrls = _s3.ResolveMealImageUrls(x.Images ?? []),
            AverageStars = x.Avg,
            TotalRatings = x.Total,
            OneStarCount = x.One,
            TwoStarCount = x.Two,
        }).ToList();
    }
}
