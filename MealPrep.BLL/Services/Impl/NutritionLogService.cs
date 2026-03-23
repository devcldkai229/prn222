using BusinessObjects.Entities;
using MealPrep.BLL.Services.Interfaces;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class DailyNutritionEntry
{
    public DateOnly Date { get; set; }
    public int TotalCalories { get; set; }
    public decimal TotalProteinG { get; set; }
    public decimal TotalCarbsG { get; set; }
    public decimal TotalFatG { get; set; }
    public List<NutritionMealEntry> Meals { get; set; } = [];
}

public class NutritionMealEntry
{
    public int MealId { get; set; }
    public string MealName { get; set; } = "";
    public string[] ImageUrls { get; set; } = [];
    public decimal Calories { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
    public int Quantity { get; set; }
}

public class WeeklyNutritionSummary
{
    public int? DailyCalorieTarget { get; set; }
    public List<DailyNutritionEntry> Days { get; set; } = [];
    public int TotalCalories => Days.Sum(d => d.TotalCalories);
    public decimal AverageCalories => Days.Count > 0 ? (decimal)TotalCalories / Days.Count : 0;
}

public class NutritionLogEntryDto
{
    public int Id { get; set; }
    public int MealId { get; set; }
    public string MealName { get; set; } = "";
    public string[] ImageUrls { get; set; } = [];
    public int Quantity { get; set; }
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
}

public class MealSelectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Calories { get; set; }
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface INutritionLogService
{
    Task LogMealAsync(Guid userId, int mealId, DateOnly date, int quantity = 1);
    Task<WeeklyNutritionSummary> GetWeeklySummaryAsync(Guid userId, DateOnly weekStart);
    Task<List<NutritionLogEntryDto>> GetLogsByDateAsync(Guid userId, DateOnly date);
    Task<List<MealSelectDto>> GetActiveMealsForLogAsync();
}

// ── Implementation ────────────────────────────────────────────────────────────

public class NutritionLogService : INutritionLogService
{
    private readonly AppDbContext _ctx;
    private readonly IS3Service _s3;

    public NutritionLogService(AppDbContext ctx, IS3Service s3)
    {
        _ctx = ctx;
        _s3 = s3;
    }

    public async Task LogMealAsync(Guid userId, int mealId, DateOnly date, int quantity = 1)
    {
        var user =
            await _ctx.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        _ctx.NutritionLogs.Add(
            new NutritionLog
            {
                UserId = userId,
                CustomerEmail = user.Email,
                MealId = mealId,
                Date = date,
                Quantity = quantity,
            }
        );
        await _ctx.SaveChangesAsync();
    }

    public async Task<WeeklyNutritionSummary> GetWeeklySummaryAsync(Guid userId, DateOnly weekStart)
    {
        var weekEnd = weekStart.AddDays(6);

        var user =
            await _ctx.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        var logs = await _ctx
            .NutritionLogs.Include(l => l.Meal)
            .Where(l => l.UserId == userId && l.Date >= weekStart && l.Date <= weekEnd)
            .ToListAsync();

        // Build 7-day structure
        var days = Enumerable
            .Range(0, 7)
            .Select(i =>
            {
                var date = weekStart.AddDays(i);
                var dayLogs = logs.Where(l => l.Date == date).ToList();

                return new DailyNutritionEntry
                {
                    Date = date,
                    TotalCalories = dayLogs.Sum(l => (int)(l.Meal?.Calories ?? 0) * l.Quantity),
                    TotalProteinG = dayLogs.Sum(l => (l.Meal?.Protein ?? 0) * l.Quantity),
                    TotalCarbsG = dayLogs.Sum(l => (l.Meal?.Carbs ?? 0) * l.Quantity),
                    TotalFatG = dayLogs.Sum(l => (l.Meal?.Fat ?? 0) * l.Quantity),
                    Meals = dayLogs
                        .Select(l => new NutritionMealEntry
                        {
                            MealId = l.MealId,
                            MealName = l.Meal?.Name ?? "",
                            ImageUrls = _s3.ResolveMealImageUrls(l.Meal?.Images ?? []),
                            Calories = (l.Meal?.Calories ?? 0) * l.Quantity,
                            ProteinG = (l.Meal?.Protein ?? 0) * l.Quantity,
                            CarbsG = (l.Meal?.Carbs ?? 0) * l.Quantity,
                            FatG = (l.Meal?.Fat ?? 0) * l.Quantity,
                            Quantity = l.Quantity,
                        })
                        .ToList(),
                };
            })
            .ToList();

        return new WeeklyNutritionSummary { DailyCalorieTarget = user.CaloriesInDay, Days = days };
    }

    public async Task<List<NutritionLogEntryDto>> GetLogsByDateAsync(Guid userId, DateOnly date)
    {
        var logs = await _ctx.NutritionLogs
            .Include(l => l.Meal)
            .Where(l => l.UserId == userId && l.Date == date)
            .OrderByDescending(l => l.Id)
            .ToListAsync();
        return logs.Select(l => new NutritionLogEntryDto
        {
            Id = l.Id,
            MealId = l.MealId,
            MealName = l.Meal?.Name ?? "",
            ImageUrls = _s3.ResolveMealImageUrls(l.Meal?.Images ?? []),
            Quantity = l.Quantity,
            Calories = (l.Meal?.Calories ?? 0) * l.Quantity,
            Protein = (l.Meal?.Protein ?? 0) * l.Quantity,
            Carbs = (l.Meal?.Carbs ?? 0) * l.Quantity,
            Fat = (l.Meal?.Fat ?? 0) * l.Quantity,
        }).ToList();
    }

    public async Task<List<MealSelectDto>> GetActiveMealsForLogAsync()
    {
        return await _ctx.Meals
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .Select(m => new MealSelectDto
            {
                Id = m.Id,
                Name = m.Name,
                Calories = m.Calories,
            })
            .ToListAsync();
    }
}
