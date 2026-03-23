using BusinessObjects.Entities;
using BusinessObjects.Enums;
using MealPrep.BLL.Services.Interfaces;
using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.BLL.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class CreateSubscriptionDto
{
    public int PlanId { get; set; }
    public DateOnly StartDate { get; set; }
    public int MealsPerDay { get; set; }
    /// <summary>Morning / Afternoon / Evening</summary>
    public string? DeliveryTimeSlot { get; set; }
}

public class SubscriptionCreatedResult
{
    public int SubscriptionId { get; set; }
    public int PaymentId { get; set; }
    public string MomoRedirectUrl { get; set; } = "";
}

public class MomoCallbackDto
{
    public string PartnerCode { get; set; } = "";
    public string OrderId { get; set; } = ""; // our PaymentCode
    public string RequestId { get; set; } = "";
    public long Amount { get; set; }
    public string OrderInfo { get; set; } = "";
    public string OrderType { get; set; } = "";
    public long TransId { get; set; }
    public int ResultCode { get; set; }
    public string Message { get; set; } = "";
    public string PayType { get; set; } = "";
    public long ResponseTime { get; set; }
    public string ExtraData { get; set; } = "";
    public string Signature { get; set; } = "";
}

public class PlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int DurationDays { get; set; }
    public int MealsPerDay { get; set; }
    public decimal BasePrice { get; set; }
    public decimal ExtraPrice { get; set; }
}

public class UserSubscriptionDto
{
    public int Id { get; set; }
    public string PlanName { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public int MealsPerDay { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? LatestPaymentStatus { get; set; }
}

public class SubscriptionDetailDto
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public string PlanName { get; set; } = "";
    public string? PlanDescription { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public int MealsPerDay { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public DateOnly? PauseFrom { get; set; }
    public DateOnly? PauseTo { get; set; }
    public List<SubscriptionPaymentDto> Payments { get; set; } = [];
}

public class SubscriptionPaymentDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "";
    public string Status { get; set; } = "";
    public string PaymentCode { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface ISubscriptionService
{
    Task<List<PlanDto>> GetActivePlansAsync();
    Task<SubscriptionCreatedResult> CreateSubscriptionWithPaymentAsync(
        Guid userId,
        CreateSubscriptionDto dto,
        string returnUrl,
        string ipnUrl
    );
    Task<(bool Success, int? SubscriptionId)> HandleMomoCallbackAsync(MomoCallbackDto callback);
    Task<(bool Success, int? SubscriptionId)> ConfirmPaymentByCallbackAsync(MomoCallbackDto callback);
    Task<Subscription?> GetActiveSubscriptionAsync(Guid userId);
    Task<List<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId);
    Task<SubscriptionDetailDto?> GetSubscriptionDetailsAsync(int id, Guid userId);
    Task CancelPendingSubscriptionAsync(int id, Guid userId);
    Task<SubscriptionCreatedResult> RetryPaymentAsync(int subscriptionId, Guid userId, string returnUrl, string ipnUrl);
    Task PauseSubscriptionAsync(int id, Guid userId, DateOnly pauseFrom, DateOnly pauseTo);
    Task ResumeSubscriptionAsync(int id, Guid userId);
    Task CancelActiveSubscriptionAsync(int id, Guid userId);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _ctx;
    private readonly IMomoService _momoService;

    public SubscriptionService(AppDbContext ctx, IMomoService momoService)
    {
        _ctx = ctx;
        _momoService = momoService;
    }

    public async Task<List<PlanDto>> GetActivePlansAsync()
    {
        return await _ctx
            .Plans.Where(p => p.IsActive)
            .OrderBy(p => p.BasePrice)
            .Select(p => new PlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DurationDays = p.DurationDays,
                MealsPerDay = p.MealsPerDay,
                BasePrice = p.BasePrice,
                ExtraPrice = p.ExtraPrice,
            })
            .ToListAsync();
    }

    public async Task<SubscriptionCreatedResult> CreateSubscriptionWithPaymentAsync(
        Guid userId,
        CreateSubscriptionDto dto,
        string returnUrl,
        string ipnUrl
    )
    {
        var user =
            await _ctx.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        var plan =
            await _ctx.Plans.FindAsync(dto.PlanId)
            ?? throw new InvalidOperationException("Plan not found.");

        if (dto.MealsPerDay < 1 || dto.MealsPerDay > 3)
            throw new InvalidOperationException("Meals per day must be between 1 and 3.");

        var totalAmount = plan.BasePrice;
        if (dto.MealsPerDay == 3 && plan.ExtraPrice > 0)
        {
            totalAmount += plan.ExtraPrice;
        }
        var endDate = dto.StartDate.AddDays(plan.DurationDays - 1);
        var paymentCode = $"MP-{Guid.NewGuid():N}".Substring(0, 20).ToUpper();

        var subscription = new Subscription
        {
            UserId = userId,
            PlanId = dto.PlanId,
            CustomerName = user.FullName,
            CustomerEmail = user.Email,
            MealsPerDay = dto.MealsPerDay,
            DeliveryTimeSlot = dto.DeliveryTimeSlot,
            StartDate = dto.StartDate,
            EndDate = endDate,
            Status = SubscriptionStatus.PendingPayment,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow,
        };
        _ctx.Subscriptions.Add(subscription);
        await _ctx.SaveChangesAsync();

        var payment = new Payment
        {
            UserId = userId,
            SubscriptionId = subscription.Id,
            Amount = totalAmount,
            Currency = "VND",
            Method = "MoMo",
            Status = "Pending",
            PaymentCode = paymentCode,
            Description = $"MealPrep subscription - {plan.Name}",
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddMinutes(15),
        };
        _ctx.Payments.Add(payment);
        await _ctx.SaveChangesAsync();

        // Generate MoMo redirect
        var redirectUrl = await _momoService.CreatePaymentRequestAsync(payment, returnUrl, ipnUrl);

        return new SubscriptionCreatedResult
        {
            SubscriptionId = subscription.Id,
            PaymentId = payment.Id,
            MomoRedirectUrl = redirectUrl,
        };
    }

    /// <summary>
    /// Called from the redirect return (PaymentReturn page).
    /// Skips signature verification — just checks resultCode and confirms payment.
    /// Returns (Success, SubscriptionId) for redirect to subscription detail.
    /// </summary>
    public async Task<(bool Success, int? SubscriptionId)> ConfirmPaymentByCallbackAsync(MomoCallbackDto callback)
    {
        if (callback.ResultCode != 0)
            return (false, null);

        var payment = await _ctx
            .Payments.Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.PaymentCode == callback.OrderId);

        if (payment == null)
            return (false, null);

        var subscriptionId = payment.SubscriptionId;

        // Idempotency guard
        if (payment.Status == "Paid")
            return (true, subscriptionId);

        var txLog = new PaymentTransaction
        {
            PaymentId = payment.Id,
            Gateway = "MoMo",
            RequestId = callback.RequestId,
            OrderId = callback.OrderId,
            ResponseCode = callback.ResultCode.ToString(),
            ResponseMessage = callback.Message,
            RawResponseJson = System.Text.Json.JsonSerializer.Serialize(callback),
            CreatedAt = DateTime.UtcNow,
        };
        _ctx.PaymentTransactions.Add(txLog);

        payment.Status = "Paid";
        payment.PaidAt = DateTime.UtcNow;

        if (payment.Subscription != null)
        {
            payment.Subscription.Status = SubscriptionStatus.Active;
            payment.Subscription.UpdatedAt = DateTime.UtcNow;
        }

        await _ctx.SaveChangesAsync();
        return (true, subscriptionId);
    }

    /// <summary>
    /// Called from IPN (server-to-server callback). Verifies signature before confirming.
    /// </summary>
    public async Task<(bool Success, int? SubscriptionId)> HandleMomoCallbackAsync(MomoCallbackDto callback)
    {
        // Build raw signature string for verification
        var rawData =
            $"accessKey={callback.PartnerCode}&amount={callback.Amount}&extraData={callback.ExtraData}&message={callback.Message}&orderId={callback.OrderId}&orderInfo={callback.OrderInfo}&orderType={callback.OrderType}&partnerCode={callback.PartnerCode}&payType={callback.PayType}&requestId={callback.RequestId}&responseTime={callback.ResponseTime}&resultCode={callback.ResultCode}&transId={callback.TransId}";

        if (!_momoService.VerifySignature(rawData, callback.Signature))
            return (false, null);

        var payment = await _ctx
            .Payments.Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.PaymentCode == callback.OrderId);

        if (payment == null)
            return (false, null);

        // Idempotency guard
        if (payment.Status == "Paid")
            return (true, payment.SubscriptionId);

        var txLog = new PaymentTransaction
        {
            PaymentId = payment.Id,
            Gateway = "MoMo",
            RequestId = callback.RequestId,
            OrderId = callback.OrderId,
            ResponseCode = callback.ResultCode.ToString(),
            ResponseMessage = callback.Message,
            RawResponseJson = System.Text.Json.JsonSerializer.Serialize(callback),
            CreatedAt = DateTime.UtcNow,
        };
        _ctx.PaymentTransactions.Add(txLog);

        if (callback.ResultCode == 0) // MoMo success
        {
            payment.Status = "Paid";
            payment.PaidAt = DateTime.UtcNow;

            if (payment.Subscription != null)
            {
                payment.Subscription.Status = SubscriptionStatus.Active;
                payment.Subscription.UpdatedAt = DateTime.UtcNow;
            }
        }
        else
        {
            payment.Status = "Failed";
        }

        await _ctx.SaveChangesAsync();
        return (callback.ResultCode == 0, payment.SubscriptionId);
    }

    public async Task<Subscription?> GetActiveSubscriptionAsync(Guid userId)
    {
        return await _ctx
            .Subscriptions.Include(s => s.Plan)
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId)
    {
        return await _ctx
            .Subscriptions.Include(s => s.Plan)
            .Include(s => s.Payments)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new UserSubscriptionDto
            {
                Id = s.Id,
                PlanName = s.Plan != null ? s.Plan.Name : "",
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Status = s.Status,
                TotalAmount = s.TotalAmount,
                MealsPerDay = s.MealsPerDay,
                CreatedAt = s.CreatedAt,
                LatestPaymentStatus = s.Payments.OrderByDescending(p => p.CreatedAt).Select(p => p.Status).FirstOrDefault(),
            })
            .ToListAsync();
    }

    public async Task<SubscriptionDetailDto?> GetSubscriptionDetailsAsync(int id, Guid userId)
    {
        var sub = await _ctx
            .Subscriptions.Include(s => s.Plan)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (sub == null) return null;

        return new SubscriptionDetailDto
        {
            Id = sub.Id,
            PlanId = sub.PlanId,
            PlanName = sub.Plan?.Name ?? "",
            PlanDescription = sub.Plan?.Description,
            StartDate = sub.StartDate,
            EndDate = sub.EndDate,
            Status = sub.Status,
            TotalAmount = sub.TotalAmount,
            MealsPerDay = sub.MealsPerDay,
            CreatedAt = sub.CreatedAt,
            CustomerName = sub.CustomerName,
            CustomerEmail = sub.CustomerEmail,
            PauseFrom = sub.PauseFrom,
            PauseTo = sub.PauseTo,
            Payments = sub.Payments.OrderByDescending(p => p.CreatedAt).Select(p => new SubscriptionPaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Method = p.Method,
                Status = p.Status,
                PaymentCode = p.PaymentCode,
                CreatedAt = p.CreatedAt,
                PaidAt = p.PaidAt,
            }).ToList(),
        };
    }

    // ── PAUSE ──────────────────────────────────────────────────────────────
    public async Task PauseSubscriptionAsync(int id, Guid userId, DateOnly pauseFrom, DateOnly pauseTo)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (pauseFrom < today)
            throw new InvalidOperationException("Ngày bắt đầu tạm ngưng không thể trong quá khứ.");
        if (pauseTo <= pauseFrom)
            throw new InvalidOperationException("Ngày kết thúc tạm ngưng phải sau ngày bắt đầu.");

        var sub = await _ctx.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId)
            ?? throw new InvalidOperationException("Không tìm thấy gói đăng ký.");

        if (sub.Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Chỉ có thể tạm ngưng gói đang hoạt động.");
        if (sub.EndDate.HasValue && pauseFrom > sub.EndDate.Value)
            throw new InvalidOperationException("Ngày tạm ngưng phải trước ngày kết thúc gói.");

        int pauseDays = pauseTo.DayNumber - pauseFrom.DayNumber + 1;
        sub.Status = SubscriptionStatus.Paused;
        sub.PauseFrom = pauseFrom;
        sub.PauseTo = pauseTo;
        sub.UpdatedAt = DateTime.UtcNow;

        // Kéo dài EndDate tương ứng số ngày nghỉ
        if (sub.EndDate.HasValue)
            sub.EndDate = sub.EndDate.Value.AddDays(pauseDays);

        // Hủy các đơn Planned/Preparing trong khoảng nghỉ
        var ordersToCancel = await _ctx.Orders
            .Where(o => o.SubscriptionId == id
                && o.DeliveryDate >= pauseFrom
                && o.DeliveryDate <= pauseTo
                && (o.Status == OrderStatus.Planned || o.Status == OrderStatus.Preparing))
            .ToListAsync();
        foreach (var order in ordersToCancel)
            order.Status = OrderStatus.Cancelled;

        await _ctx.SaveChangesAsync();
    }

    // ── RESUME ─────────────────────────────────────────────────────────────
    public async Task ResumeSubscriptionAsync(int id, Guid userId)
    {
        var sub = await _ctx.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId)
            ?? throw new InvalidOperationException("Không tìm thấy gói đăng ký.");

        if (sub.Status != SubscriptionStatus.Paused)
            throw new InvalidOperationException("Gói không ở trạng thái tạm ngưng.");

        // Nếu resume sớm, rút lại phần EndDate đã kéo nhưng chưa dùng
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (sub.PauseTo.HasValue && today < sub.PauseTo.Value && sub.EndDate.HasValue)
        {
            int unusedDays = sub.PauseTo.Value.DayNumber - today.DayNumber;
            sub.EndDate = sub.EndDate.Value.AddDays(-unusedDays);
        }

        sub.Status = SubscriptionStatus.Active;
        sub.PauseFrom = null;
        sub.PauseTo = null;
        sub.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }

    // ── CANCEL ACTIVE ──────────────────────────────────────────────────────
    public async Task CancelActiveSubscriptionAsync(int id, Guid userId)
    {
        var sub = await _ctx.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId)
            ?? throw new InvalidOperationException("Không tìm thấy gói đăng ký.");

        if (sub.Status != SubscriptionStatus.Active && sub.Status != SubscriptionStatus.Paused)
            throw new InvalidOperationException("Chỉ có thể hủy gói đang hoạt động hoặc tạm ngưng.");

        sub.Status = SubscriptionStatus.Cancelled;
        sub.UpdatedAt = DateTime.UtcNow;

        // Hủy các đơn tương lai chưa giao
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var futureOrders = await _ctx.Orders
            .Where(o => o.SubscriptionId == id
                && o.DeliveryDate > today
                && (o.Status == OrderStatus.Planned || o.Status == OrderStatus.Preparing))
            .ToListAsync();
        foreach (var order in futureOrders)
            order.Status = OrderStatus.Cancelled;

        await _ctx.SaveChangesAsync();
    }

    public async Task CancelPendingSubscriptionAsync(int id, Guid userId)
    {
        var sub = await _ctx
            .Subscriptions.Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId)
            ?? throw new InvalidOperationException("Không tìm thấy gói đăng ký.");

        if (sub.Status != SubscriptionStatus.PendingPayment)
            throw new InvalidOperationException("Chỉ có thể hủy gói đang chờ thanh toán.");

        if (sub.Payments.Any(p => p.Status == "Paid"))
            throw new InvalidOperationException("Gói này đã được thanh toán, không thể hủy.");

        sub.Status = SubscriptionStatus.Cancelled;
        sub.UpdatedAt = DateTime.UtcNow;

        foreach (var payment in sub.Payments.Where(p => p.Status == "Pending"))
        {
            payment.Status = "Cancelled";
            payment.ExpiredAt = DateTime.UtcNow;
        }

        await _ctx.SaveChangesAsync();
    }

    public async Task<SubscriptionCreatedResult> RetryPaymentAsync(
        int subscriptionId, Guid userId, string returnUrl, string ipnUrl)
    {
        var sub = await _ctx
            .Subscriptions.Include(s => s.Plan).Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId)
            ?? throw new InvalidOperationException("Không tìm thấy gói đăng ký.");

        if (sub.Status != SubscriptionStatus.PendingPayment)
            throw new InvalidOperationException("Chỉ có thể thanh toán lại gói đang chờ thanh toán.");

        // Cancel old pending payments
        foreach (var p in sub.Payments.Where(p => p.Status == "Pending"))
        {
            p.Status = "Cancelled";
            p.ExpiredAt = DateTime.UtcNow;
        }

        // Create new payment
        var payment = new Payment
        {
            UserId = userId,
            SubscriptionId = sub.Id,
            Amount = sub.TotalAmount,
            Currency = "VND",
            Method = "MoMo",
            Status = "Pending",
            PaymentCode = $"MP-{Guid.NewGuid():N}"[..20].ToUpper(),
            Description = $"MealPrep - Thanh toán lại gói {sub.Plan?.Name}",
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddMinutes(15),
        };
        _ctx.Payments.Add(payment);
        await _ctx.SaveChangesAsync();

        var redirectUrl = await _momoService.CreatePaymentRequestAsync(payment, returnUrl, ipnUrl);

        return new SubscriptionCreatedResult
        {
            SubscriptionId = sub.Id,
            PaymentId = payment.Id,
            MomoRedirectUrl = redirectUrl,
        };
    }
}
