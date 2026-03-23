using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.BLL.Services.Interfaces;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class UpdateNutritionDto
{
    public int HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public FitnessGoal Goal { get; set; }
    public ActivityLevel ActivityLevel { get; set; }
    public DietPreference DietPreference { get; set; }
    public int? CaloriesInDay { get; set; }
    public string[] Allergies { get; set; } = [];
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? PhoneNumber { get; set; }
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string? DeliveryAddress { get; set; }
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public int HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public FitnessGoal Goal { get; set; }
    public ActivityLevel ActivityLevel { get; set; }
    public DietPreference DietPreference { get; set; }
    public int? CaloriesInDay { get; set; }
    public string[] Allergies { get; set; } = [];
    public List<DislikedMealDto> DislikedMeals { get; set; } = [];
}

public class DislikedMealDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string[] ImageUrls { get; set; } = [];
    public decimal Calories { get; set; }
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IUserService
{
    Task<UserProfileDto> GetUserProfileAsync(Guid userId);
    Task UpdateNutritionProfileAsync(Guid userId, UpdateNutritionDto dto);
    Task AddDislikedMealAsync(Guid userId, int mealId);
    Task RemoveDislikedMealAsync(Guid userId, int mealId);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class UserService : IUserService
{
    private readonly AppDbContext _ctx;
    private readonly IS3Service _s3;

    public UserService(AppDbContext ctx, IS3Service s3)
    {
        _ctx = ctx;
        _s3 = s3;
    }

    public async Task<UserProfileDto> GetUserProfileAsync(Guid userId)
    {
        var user =
            await _ctx.Users.Include(u => u.DislikedMeals).FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found.");

        return MapToDto(user);
    }

    public async Task UpdateNutritionProfileAsync(Guid userId, UpdateNutritionDto dto)
    {
        var user =
            await _ctx.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        user.HeightCm = dto.HeightCm;
        user.WeightKg = dto.WeightKg;
        user.Goal = dto.Goal;
        user.ActivityLevel = dto.ActivityLevel;
        user.DietPreference = dto.DietPreference;
        user.CaloriesInDay = dto.CaloriesInDay;
        user.Allergies = dto.Allergies;
        user.Gender = dto.Gender;
        user.Age = dto.Age;
        user.DeliveryAddress = dto.DeliveryAddress;
        user.PhoneNumber = dto.PhoneNumber;

        await _ctx.SaveChangesAsync();
    }

    public async Task AddDislikedMealAsync(Guid userId, int mealId)
    {
        var user =
            await _ctx.Users.Include(u => u.DislikedMeals).FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found.");

        var meal =
            await _ctx.Meals.FindAsync(mealId)
            ?? throw new InvalidOperationException("Meal not found.");

        user.DislikedMeals ??= new List<Meal>();
        if (!user.DislikedMeals.Any(m => m.Id == mealId))
        {
            user.DislikedMeals.Add(meal);
            await _ctx.SaveChangesAsync();
        }
    }

    public async Task RemoveDislikedMealAsync(Guid userId, int mealId)
    {
        var user =
            await _ctx.Users.Include(u => u.DislikedMeals).FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found.");

        var meal = user.DislikedMeals?.FirstOrDefault(m => m.Id == mealId);
        if (meal != null)
        {
            user.DislikedMeals!.Remove(meal);
            await _ctx.SaveChangesAsync();
        }
    }

    private UserProfileDto MapToDto(User user) =>
        new()
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            PhoneNumber = user.PhoneNumber,
            DeliveryAddress = user.DeliveryAddress,
            Gender = user.Gender,
            Age = user.Age,
            HeightCm = user.HeightCm,
            WeightKg = user.WeightKg,
            Goal = user.Goal,
            ActivityLevel = user.ActivityLevel,
            DietPreference = user.DietPreference,
            CaloriesInDay = user.CaloriesInDay,
            Allergies = user.Allergies ?? [],
            DislikedMeals =
                user.DislikedMeals?.Select(m => new DislikedMealDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        ImageUrls = _s3.ResolveMealImageUrls(m.Images ?? []),
                        Calories = m.Calories,
                    })
                    .ToList()
                ?? [],
        };
}
