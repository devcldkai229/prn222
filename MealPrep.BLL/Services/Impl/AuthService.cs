using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.BLL.Exceptions;
using MealPrep.BLL.Services.Interfaces;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

public interface IAuthService
{
    Task SendOtpAsync(string email);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(string email, string password);
}

public record AuthResponse(
    Guid UserId,
    string Email,
    string FullName,
    Guid AppRoleId,
    string RoleName,
    bool IsActive,
    string AvatarUrl
);

public record RegisterRequest(string Email, string FullName, string Password, string OtpCode);

public class AuthService : IAuthService
{
    private readonly AppDbContext _ctx;
    private readonly IEmailService _emailService;
    private static readonly Guid UserRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private const int OTP_EXPIRY_MINUTES = 10;

    public AuthService(AppDbContext ctx, IEmailService emailService)
    {
        _ctx = ctx;
        _emailService = emailService;
    }

    public async Task SendOtpAsync(string email)
    {
        if (await _ctx.Users.AnyAsync(u => u.Email == email))
            throw new EmailAlreadyExistsException(email);

        // Delete old unused OTPs for this email
        var oldOtps = await _ctx.OtpCodes.Where(o => o.Email == email).ToListAsync();
        _ctx.OtpCodes.RemoveRange(oldOtps);

        var code = new Random().Next(100000, 999999).ToString();
        _ctx.OtpCodes.Add(
            new OtpCode
            {
                Id = Guid.NewGuid(),
                Email = email,
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES),
                IsUsed = false,
            }
        );
        await _ctx.SaveChangesAsync();

        await _emailService.SendOtpEmailAsync(email, code);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // 1. Verify OTP
        var otp =
            await _ctx
                .OtpCodes.Where(o => o.Email == request.Email && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync()
            ?? throw new OtpNotSentException(request.Email);

        if (otp.ExpiresAt < DateTime.UtcNow || otp.Code != request.OtpCode)
            throw new InvalidOtpException();

        if (await _ctx.Users.AnyAsync(u => u.Email == request.Email))
            throw new EmailAlreadyExistsException(request.Email);

        // 2. Create user
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = passwordHash,
            AppRoleId = UserRoleId,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true,
        };

        otp.IsUsed = true;
        _ctx.Users.Add(user);
        await _ctx.SaveChangesAsync();

        var role = await _ctx.AppRoles.FindAsync(UserRoleId);
        return new AuthResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.AppRoleId,
            role?.Name.ToString() ?? "User",
            user.IsActive,
            user.AvatarUrl
        );
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var user =
            await _ctx.Users.Include(u => u.AppRole).FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new InvalidCredentialsException();

        if (!user.IsActive)
            throw new AccountDeactivatedException();

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new InvalidCredentialsException();

        user.LastLoginAtUtc = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.AppRoleId,
            user.AppRole.Name.ToString(),
            user.IsActive,
            user.AvatarUrl
        );
    }
}
