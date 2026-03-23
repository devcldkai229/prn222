using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MealPrep.BLL.Services
{
    public interface IAiMenuService
    {
        /// <summary>
        /// Generate AI menu recommendations without saving (for user review)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="remainingDates">List of dates that need meal planning (only future dates without orders)</param>
        /// <param name="weeklyNotes">Optional notes for this week. If provided, will override profile.Notes</param>
        Task<List<AiMenuPlanItem>> GenerateMenuAsync(
            Guid userId,
            List<DateOnly> remainingDates,
            string? weeklyNotes = null
        );

        /// <summary>
        /// Generate and save menu directly to database
        /// </summary>
        Task<int> GenerateAndSaveMenuAsync(Guid userId, DateOnly weekStart);
    }

    public class AiMenuService : IAiMenuService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _aiServiceUrl;

        public AiMenuService(
            AppDbContext context,
            IConfiguration config,
            IHttpClientFactory httpClientFactory
        )
        {
            _context = context;
            _httpClientFactory = httpClientFactory;

            // Lấy URL từ appsettings.json
            // Local: "http://localhost:8000/api/generate-menu"
            // Lambda: "https://abcdefgh.lambda-url.us-east-1.on.aws/api/generate-menu"
            _aiServiceUrl =
                config["AiSettings:ServiceUrl"]
                ?? throw new Exception("AI Service URL not configured");
        }

        /// <summary>
        /// Generate AI menu recommendations without saving (for user review)
        /// Only generates for remaining dates (future dates without confirmed orders)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="remainingDates">List of dates that need meal planning (only future dates without orders)</param>
        /// <param name="weeklyNotes">Optional notes for this week. If provided, will override profile.Notes</param>
        public async Task<List<AiMenuPlanItem>> GenerateMenuAsync(
            Guid userId,
            List<DateOnly> remainingDates,
            string? weeklyNotes = null
        )
        {
            // 1. Get User with all required data
            var user = await _context
                .Users.Include(u => u.DislikedMeals)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found.");

            // 2. Calculate calories if not set
            var caloriesInDay = user.CaloriesInDay ?? CalculateTDEE(user);

            // 3. Get allergies — loại bỏ null/empty để tránh [null] gây lỗi 422
            var allergies = user.Allergies?
                .Where(a => !string.IsNullOrEmpty(a))
                .ToList() ?? new List<string>();

            // 4. Determine notes
            var notesToUse = !string.IsNullOrWhiteSpace(weeklyNotes)
                ? weeklyNotes.Trim()
                : (user.Hobbies ?? ""); // Hobbies used as general notes field

            var numberOfDays = remainingDates.Count;

            // 5. Determine meals per day based on active subscription (fallback to 3)
            var mealsPerDay = 3;
            if (remainingDates.Count > 0)
            {
                var minDate = remainingDates.Min();
                var maxDate = remainingDates.Max();

                var activeSubs = await _context
                    .Subscriptions.Where(s =>
                        s.UserId == userId && s.Status == SubscriptionStatus.Active
                    )
                    .OrderBy(s => s.StartDate)
                    .ToListAsync();

                var matchedSub = activeSubs.FirstOrDefault(s =>
                    s.StartDate <= minDate && (s.EndDate == null || s.EndDate >= maxDate)
                );

                if (matchedSub != null && matchedSub.MealsPerDay >= 1 && matchedSub.MealsPerDay <= 3)
                {
                    mealsPerDay = matchedSub.MealsPerDay;
                }
            }

            var payload = new GenerateMenuRequestDto
            {
                UserProfile = new AiMenuUserProfileDto
                {
                    HeightCm = user.HeightCm,
                    WeightKg = (double)user.WeightKg,
                    Goal = (int)user.Goal,
                    ActivityLevel = (int)user.ActivityLevel,
                    MealsPerDay = mealsPerDay,
                    Notes = notesToUse ?? "",
                    DietPref = (int)user.DietPreference,
                    CaloriesInDay = caloriesInDay,
                    Allergies = allergies,
                },
                DislikedIds = user.DislikedMeals?.Select(d => d.Id).ToList() ?? new List<int>(),
                NumberOfDays = numberOfDays,
                WeeklyPreference = notesToUse ?? "", // Ưu tiên: dùng làm instruction chính cho AI (vd: "nhiều thịt bò, có rau")
            };

            // 5. Gọi AI Service (Python) - serialization snake_case khớp Pydantic
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            };
            var jsonPayload = JsonSerializer.Serialize(payload, jsonOptions);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(2); // AI service may take time

            var response = await httpClient.PostAsync(_aiServiceUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"AI Service failed: {error}");
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            var menuPlan = JsonSerializer.Deserialize<List<AiMenuPlanItem>>(
                resultJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (menuPlan == null || !menuPlan.Any())
            {
                throw new Exception("AI Service returned empty or invalid menu plan");
            }

            return menuPlan;
        }

        /// <summary>
        /// Generate and save menu directly to database
        /// Note: This method is deprecated. Use GenerateMenuAsync with remainingDates instead.
        /// </summary>
        public async Task<int> GenerateAndSaveMenuAsync(Guid userId, DateOnly weekStart)
        {
            // Build a list of 7 days from weekStart for backward compatibility
            var dates = new List<DateOnly>();
            for (int i = 0; i < 7; i++)
            {
                dates.Add(weekStart.AddDays(i));
            }

            var menuPlan = await GenerateMenuAsync(userId, dates);
            return await SaveMenuToDb(userId, weekStart, menuPlan);
        }

        private async Task<int> SaveMenuToDb(
            Guid userId,
            DateOnly weekStart,
            List<AiMenuPlanItem> plan
        )
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var menu = new WeeklyMenu
                {
                    UserId = userId,
                    WeekStart = weekStart,
                    WeekEnd = weekStart.AddDays(6),
                };
                _context.WeeklyMenus.Add(menu);
                await _context.SaveChangesAsync();

                foreach (var day in plan)
                {
                    foreach (var mealId in day.meal_ids)
                    {
                        _context.WeeklyMenuItems.Add(
                            new WeeklyMenuItem
                            {
                                WeeklyMenuId = menu.Id,
                                MealId = mealId,
                                DayOfWeek = day.day,
                            }
                        );
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return menu.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Tính TDEE (Total Daily Energy Expenditure) dựa trên BMR và Activity Level
        /// Sử dụng Mifflin-St Jeor Equation
        /// </summary>
        private int CalculateTDEE(User user)
        {
            // BMR (Basal Metabolic Rate) - Mifflin-St Jeor Equation
            // BMR = 10 * weight(kg) + 6.25 * height(cm) - 5 * age + s
            // s = +5 (male) or -161 (female)
            var age = user.Age > 0 ? user.Age : 30;
            var genderFactor = user.Gender == BusinessObjects.Enums.Gender.Female ? -161 : 5;
            var bmr = 10 * (double)user.WeightKg + 6.25 * user.HeightCm - 5 * age + genderFactor;

            var activityMultipliers = new Dictionary<int, double>
            {
                { 1, 1.2 },
                { 2, 1.375 },
                { 3, 1.55 },
                { 4, 1.725 },
                { 5, 1.9 },
            };

            var multiplier = activityMultipliers.GetValueOrDefault((int)user.ActivityLevel, 1.55);
            var tdee = bmr * multiplier;

            switch (user.Goal)
            {
                case BusinessObjects.Enums.FitnessGoal.FatLoss:
                    tdee *= 0.85;
                    break;
                case BusinessObjects.Enums.FitnessGoal.MuscleGain:
                    tdee *= 1.15;
                    break;
            }

            return (int)Math.Round(tdee);
        }
    }

    /// <summary>
    /// Request DTO khớp chính xác với Python Pydantic GenerateMenuRequest.
    /// Dùng JsonPropertyName để đảm bảo serialization snake_case.
    /// </summary>
    internal class GenerateMenuRequestDto
    {
        [JsonPropertyName("user_profile")]
        public AiMenuUserProfileDto UserProfile { get; set; } = null!;

        [JsonPropertyName("disliked_ids")]
        public List<int> DislikedIds { get; set; } = new();

        [JsonPropertyName("number_of_days")]
        public int NumberOfDays { get; set; } = 7;

        /// <summary>Ưu tiên cao: user nhập "Bạn muốn ăn thế nào" - BẮT BUỘC AI phải tuân theo (vd: nhiều thịt bò, có rau, ít gà).</summary>
        [JsonPropertyName("weekly_preference")]
        public string WeeklyPreference { get; set; } = "";
    }

    internal class AiMenuUserProfileDto
    {
        [JsonPropertyName("height_cm")]
        public int HeightCm { get; set; }

        [JsonPropertyName("weight_kg")]
        public double WeightKg { get; set; }

        [JsonPropertyName("goal")]
        public int Goal { get; set; }

        [JsonPropertyName("activity_level")]
        public int ActivityLevel { get; set; }

        [JsonPropertyName("meals_per_day")]
        public int MealsPerDay { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; } = "";

        [JsonPropertyName("diet_pref")]
        public int DietPref { get; set; }

        [JsonPropertyName("calories_in_day")]
        public int CaloriesInDay { get; set; }

        [JsonPropertyName("allergies")]
        public List<string> Allergies { get; set; } = new();
    }

    // DTO Helper
    public class AiMenuPlanItem
    {
        public int day { get; set; }
        public List<int> meal_ids { get; set; } = new();
        public string reason { get; set; } = string.Empty;
    }
}
