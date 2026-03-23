using System.Security.Claims;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Profile;

[Authorize]
public class NutritionModel : PageModel
{
    private readonly IUserService _userService;

    public NutritionModel(IUserService userService) => _userService = userService;

    public UserProfileDto Profile { get; private set; } = null!;

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync()
    {
        Profile = await _userService.GetUserProfileAsync(UserId);
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync(UpdateNutritionDto dto)
    {
        if (!ModelState.IsValid)
        {
            Profile = await _userService.GetUserProfileAsync(UserId);
            return Page();
        }

        await _userService.UpdateNutritionProfileAsync(UserId, dto);
        SuccessMessage = "Profile updated successfully!";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnblockMealAsync(int mealId)
    {
        await _userService.RemoveDislikedMealAsync(UserId, mealId);
        return new JsonResult(new { success = true, message = "Meal unblocked!" });
    }
}
