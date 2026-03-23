using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BusinessObjects.Enums;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Auth;

[Authorize]
public class CompleteProfileModel : PageModel
{
    private readonly IUserService _userService;

    public CompleteProfileModel(IUserService userService) => _userService = userService;

    [BindProperty]
    public ProfileInput Input { get; set; } = new();

    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ErrorMessage { get; set; }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync()
    {
        var profile = await _userService.GetUserProfileAsync(UserId);
        Input.PhoneNumber = profile.PhoneNumber ?? "";
        Input.Gender = profile.Gender;
        Input.Age = profile.Age > 0 ? profile.Age : null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            var profile = await _userService.GetUserProfileAsync(UserId);
            await _userService.UpdateNutritionProfileAsync(UserId, new UpdateNutritionDto
            {
                HeightCm = profile.HeightCm,
                WeightKg = profile.WeightKg,
                Goal = profile.Goal,
                ActivityLevel = profile.ActivityLevel,
                DietPreference = profile.DietPreference,
                CaloriesInDay = profile.CaloriesInDay,
                Allergies = profile.Allergies,
                Gender = Input.Gender,
                Age = Input.Age ?? 0,
                DeliveryAddress = profile.DeliveryAddress,
                PhoneNumber = Input.PhoneNumber,
            });

            SuccessMessage = "Hoàn tất thông tin cá nhân thành công!";
            return RedirectToPage("/Auth/SetupNutrition");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            return Page();
        }
    }

    public class ProfileInput
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tuổi")]
        [Range(10, 120, ErrorMessage = "Tuổi phải từ 10 đến 120")]
        public int? Age { get; set; }
    }
}
