using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class CreatePlanDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int DurationDays { get; set; }
    public int MealsPerDay { get; set; }
    public decimal BasePrice { get; set; }
    public decimal ExtraPrice { get; set; }
    public bool IsActive { get; set; } = true;
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IPlanService
{
    Task<List<Plan>> GetAllAsync();
    Task<Plan?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreatePlanDto dto);
    Task UpdateAsync(int id, CreatePlanDto dto);
    Task<(bool Success, string? Error)> ToggleActiveAsync(int id);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class PlanService : IPlanService
{
    private readonly AppDbContext _ctx;

    public PlanService(AppDbContext ctx) => _ctx = ctx;

    public Task<List<Plan>> GetAllAsync() =>
        _ctx.Plans.OrderByDescending(p => p.CreatedAt).ToListAsync();

    public Task<Plan?> GetByIdAsync(int id) => _ctx.Plans.FindAsync(id).AsTask();

    public async Task<int> CreateAsync(CreatePlanDto dto)
    {
        var plan = new Plan
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            DurationDays = dto.DurationDays,
            MealsPerDay = dto.MealsPerDay,
            BasePrice = dto.BasePrice,
            ExtraPrice = dto.ExtraPrice,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
        };
        _ctx.Plans.Add(plan);
        await _ctx.SaveChangesAsync();
        return plan.Id;
    }

    public async Task UpdateAsync(int id, CreatePlanDto dto)
    {
        var plan =
            await _ctx.Plans.FindAsync(id)
            ?? throw new InvalidOperationException("Plan not found.");

        plan.Name = dto.Name.Trim();
        plan.Description = dto.Description?.Trim();
        plan.DurationDays = dto.DurationDays;
        plan.MealsPerDay = dto.MealsPerDay;
        plan.BasePrice = dto.BasePrice;
        plan.ExtraPrice = dto.ExtraPrice;
        plan.IsActive = dto.IsActive;

        await _ctx.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> ToggleActiveAsync(int id)
    {
        var plan = await _ctx.Plans.FindAsync(id);
        if (plan == null)
            return (false, "Plan not found.");

        // Guard: cannot deactivate if active subscriptions exist
        if (plan.IsActive)
        {
            var hasActiveSubs = await _ctx.Subscriptions.AnyAsync(s =>
                s.PlanId == id && s.Status == SubscriptionStatus.Active
            );
            if (hasActiveSubs)
                return (
                    false,
                    "Cannot deactivate — plan has active subscribers. Cancel or migrate subscriptions first."
                );
        }

        plan.IsActive = !plan.IsActive;
        await _ctx.SaveChangesAsync();
        return (true, null);
    }
}
