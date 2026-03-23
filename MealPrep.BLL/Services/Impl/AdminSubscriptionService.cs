using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class SubscriptionSummaryDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string PlanName { get; set; } = "";
    public int MealsPerDay { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrderCount { get; set; }
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IAdminSubscriptionService
{
    Task<PagedResult<SubscriptionSummaryDto>> GetPagedAsync(
        SubscriptionStatus? status,
        string? emailSearch,
        int page,
        int pageSize
    );
    Task OverrideStatusAsync(int subscriptionId, SubscriptionStatus newStatus);
    Task<SubscriptionSummaryDto?> GetByIdAsync(int subscriptionId);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class AdminSubscriptionService : IAdminSubscriptionService
{
    private readonly AppDbContext _ctx;

    public AdminSubscriptionService(AppDbContext ctx) => _ctx = ctx;

    public async Task<PagedResult<SubscriptionSummaryDto>> GetPagedAsync(
        SubscriptionStatus? status,
        string? emailSearch,
        int page,
        int pageSize
    )
    {
        var q = _ctx.Subscriptions.Include(s => s.Plan).AsQueryable();

        if (status.HasValue)
            q = q.Where(s => s.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(emailSearch))
        {
            var lower = emailSearch.ToLower();
            q = q.Where(s =>
                s.CustomerEmail.ToLower().Contains(lower)
                || s.CustomerName.ToLower().Contains(lower)
            );
        }

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var subIds = items.Select(s => s.Id).ToList();
        var orderCounts = await _ctx
            .Orders.Where(o => subIds.Contains(o.SubscriptionId))
            .GroupBy(o => o.SubscriptionId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        return new PagedResult<SubscriptionSummaryDto>
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Items = items
                .Select(s => new SubscriptionSummaryDto
                {
                    Id = s.Id,
                    CustomerName = s.CustomerName,
                    CustomerEmail = s.CustomerEmail,
                    PlanName = s.Plan?.Name ?? "—",
                    MealsPerDay = s.MealsPerDay,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Status = s.Status,
                    TotalAmount = s.TotalAmount,
                    CreatedAt = s.CreatedAt,
                    OrderCount = orderCounts.GetValueOrDefault(s.Id),
                })
                .ToList(),
        };
    }

    public async Task OverrideStatusAsync(int subscriptionId, SubscriptionStatus newStatus)
    {
        var sub =
            await _ctx.Subscriptions.FindAsync(subscriptionId)
            ?? throw new InvalidOperationException("Subscription not found.");
        sub.Status = newStatus;
        sub.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }

    public async Task<SubscriptionSummaryDto?> GetByIdAsync(int subscriptionId)
    {
        var s = await _ctx
            .Subscriptions.Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);

        if (s == null)
            return null;

        return new SubscriptionSummaryDto
        {
            Id = s.Id,
            CustomerName = s.CustomerName,
            CustomerEmail = s.CustomerEmail,
            PlanName = s.Plan?.Name ?? "—",
            MealsPerDay = s.MealsPerDay,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Status = s.Status,
            TotalAmount = s.TotalAmount,
            CreatedAt = s.CreatedAt,
        };
    }
}
