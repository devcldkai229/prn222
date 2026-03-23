using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public string? RoleName { get; set; }
    public int SubscriptionCount { get; set; }
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IAdminUserService
{
    Task<PagedResult<UserSummaryDto>> GetUsersPagedAsync(
        string? search,
        bool? isActive,
        int page,
        int pageSize
    );
    Task DeactivateAccountAsync(Guid userId);
    Task ReactivateAccountAsync(Guid userId);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class AdminUserService : IAdminUserService
{
    private readonly AppDbContext _ctx;

    public AdminUserService(AppDbContext ctx) => _ctx = ctx;

    public async Task<PagedResult<UserSummaryDto>> GetUsersPagedAsync(
        string? search,
        bool? isActive,
        int page,
        int pageSize
    )
    {
        var q = _ctx.Users.Include(u => u.AppRole).AsQueryable();

        if (isActive.HasValue)
            q = q.Where(u => u.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            q = q.Where(u =>
                u.Email.ToLower().Contains(lower) || u.FullName.ToLower().Contains(lower)
            );
        }

        var total = await q.CountAsync();

        var users = await q.OrderByDescending(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userIds = users.Select(u => u.Id).ToList();
        var subCounts = await _ctx
            .Subscriptions.Where(s => userIds.Contains(s.UserId))
            .GroupBy(s => s.UserId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        return new PagedResult<UserSummaryDto>
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Items = users
                .Select(u => new UserSummaryDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    CreatedAtUtc = u.CreatedAtUtc,
                    LastLoginAtUtc = u.LastLoginAtUtc,
                    RoleName = u.AppRole?.Name.ToString(),
                    SubscriptionCount = subCounts.GetValueOrDefault(u.Id),
                })
                .ToList(),
        };
    }

    public async Task DeactivateAccountAsync(Guid userId)
    {
        var user =
            await _ctx.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");
        user.IsActive = false;
        await _ctx.SaveChangesAsync();
    }

    public async Task ReactivateAccountAsync(Guid userId)
    {
        var user =
            await _ctx.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");
        user.IsActive = true;
        await _ctx.SaveChangesAsync();
    }
}
