using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BusinessObjects.Enums;
using MealPrep.BLL.Exceptions;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService) => _authService = authService;

    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(GetRedirectUrl());
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var result = await _authService.LoginAsync(Input.Email, Input.Password);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new(ClaimTypes.Email, result.Email),
                new(ClaimTypes.Name, result.FullName),
                new(ClaimTypes.Role, result.RoleName),
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = Input.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                }
            );

            var redirect = GetRedirectUrl(result.RoleName);
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                redirect = ReturnUrl;
            return Redirect(redirect);
        }
        catch (InvalidCredentialsException)
        {
            ErrorMessage = "Invalid email or password. Please try again.";
        }
        catch (AccountDeactivatedException)
        {
            ErrorMessage = "Your account has been deactivated. Please contact support.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }

        return Page();
    }

    private string GetRedirectUrl(string? role = null)
    {
        if (role == Role.Admin.ToString())
            return "/Admin";
        if (role == "Shipper")
            return "/Shipper/Dashboard";
        return "/Dashboard";
    }

    public class LoginInputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }
}
