using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BusinessObjects.Enums;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Auth;

[Authorize]
public class SetupNutritionModel : PageModel
{
    private readonly IUserService _userService;

    public SetupNutritionModel(IUserService userService) => _userService = userService;

    [BindProperty]
    public NutritionInput Input { get; set; } = new();

    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ErrorMessage { get; set; }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync()
    {
        var profile = await _userService.GetUserProfileAsync(UserId);
        Input.HeightCm = profile.HeightCm > 0 ? profile.HeightCm : null;
        Input.WeightKg = profile.WeightKg > 0 ? profile.WeightKg : null;
        Input.Goal = profile.Goal;
        Input.ActivityLevel = profile.ActivityLevel;
        Input.DietPreference = profile.DietPreference;
        Input.CaloriesInDay = profile.CaloriesInDay;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            var profile = await _userService.GetUserProfileAsync(UserId);
            await _userService.UpdateNutritionProfileAsync(UserId, new UpdateNutritionDto
            {
                HeightCm = Input.HeightCm ?? 0,
                WeightKg = Input.WeightKg ?? 0,
                Goal = Input.Goal,
                ActivityLevel = Input.ActivityLevel,
                DietPreference = Input.DietPreference,
                CaloriesInDay = Input.CaloriesInDay,
                Allergies = profile.Allergies,
                Gender = profile.Gender,
                Age = profile.Age,
                DeliveryAddress = profile.DeliveryAddress,
                PhoneNumber = profile.PhoneNumber,
            });

            SuccessMessage = "Thiết lập hồ sơ dinh dưỡng thành công!";
            return RedirectToPage("/Dashboard/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            return Page();
        }
    }

    public class NutritionInput
    {
        [Required(ErrorMessage = "Vui lòng nhập chiều cao")]
        [Range(100, 250, ErrorMessage = "Chiều cao từ 100 đến 250 cm")]
        public int? HeightCm { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập cân nặng")]
        [Range(30, 200, ErrorMessage = "Cân nặng từ 30 đến 200 kg")]
        public decimal? WeightKg { get; set; }

        public FitnessGoal Goal { get; set; }
        public ActivityLevel ActivityLevel { get; set; }
        public DietPreference DietPreference { get; set; }

        [Range(800, 5000, ErrorMessage = "Lượng calo phải từ 800 đến 5000")]
        public int? CaloriesInDay { get; set; }
    }
}
