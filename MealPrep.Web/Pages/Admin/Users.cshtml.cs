using BusinessObjects.Enums;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsersModel : PageModel
{
    private readonly IAdminUserService _userService;

    public UsersModel(IAdminUserService userService) => _userService = userService;

    public PagedResult<UserSummaryDto> Result { get; private set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;
    public const int PageSize = 15;

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        Result = await _userService.GetUsersPagedAsync(Search, IsActive, PageIndex, PageSize);
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid userId)
    {
        await _userService.DeactivateAccountAsync(userId);
        SuccessMessage = "User deactivated successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReactivateAsync(Guid userId)
    {
        await _userService.ReactivateAccountAsync(userId);
        SuccessMessage = "User reactivated successfully.";
        return RedirectToPage();
    }
}
