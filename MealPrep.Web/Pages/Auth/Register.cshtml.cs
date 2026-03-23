using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MealPrep.BLL.Exceptions;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.Web.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;

    public RegisterModel(IAuthService authService, IWebHostEnvironment env)
    {
        _authService = authService;
        _env = env;
    }

    [BindProperty]
    public RegisterInputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public IActionResult OnGet() =>
        User.Identity?.IsAuthenticated == true ? Redirect("/Dashboard") : Page();

    public async Task<IActionResult> OnPostSendOtpAsync()
    {
        if (
            string.IsNullOrWhiteSpace(Input.Email)
            || !new EmailAddressAttribute().IsValid(Input.Email)
        )
        {
            return new JsonResult(
                new { success = false, message = "Please enter a valid email address." }
            );
        }

        try
        {
            await _authService.SendOtpAsync(Input.Email);

            // In Development mode, read the OTP from DB and return it so devs
            // don't need to check their inbox during local testing.
            string? devOtp = null;
            if (_env.IsDevelopment())
            {
                using var scope = HttpContext.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MealPrep.DAL.Data.AppDbContext>();
                var otp = await db.OtpCodes
                    .Where(o => o.Email == Input.Email && !o.IsUsed)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();
                devOtp = otp?.Code;
            }

            return new JsonResult(new
            {
                success = true,
                message = $"OTP sent to {Input.Email}. Please check your inbox.",
                devOtp  // null in production, actual code in development
            });
        }
        catch (EmailAlreadyExistsException)
        {
            return new JsonResult(
                new
                {
                    success = false,
                    message = "This email is already registered. Please log in instead.",
                }
            );
        }
        catch (Exception ex)
        {
            return new JsonResult(
                new { success = false, message = $"Error: {ex.Message}" }
            );
        }
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var result = await _authService.RegisterAsync(
                new RegisterRequest(Input.Email, Input.FullName, Input.Password, Input.OtpCode)
            );

            // Auto sign-in after registration
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
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            return RedirectToPage("/Auth/CompleteProfile");
        }
        catch (InvalidOtpException)
        {
            ErrorMessage = "Invalid or expired OTP code. Please request a new one.";
        }
        catch (OtpNotSentException)
        {
            ErrorMessage = "Please request an OTP first by clicking 'Send OTP'.";
        }
        catch (EmailAlreadyExistsException)
        {
            ErrorMessage = "This email is already registered.";
        }

        return Page();
    }

    public class RegisterInputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";

        [Required, StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits.")]
        public string OtpCode { get; set; } = "";
    }
}
