using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MealPrep.DAL.Data;

public static class DatabaseSeeder
{
    private static readonly DateOnly SeedStartDate = new(2023, 1, 1);

    /// <summary>
    /// Seeds default roles and an admin user if the database is empty.
    /// Admin: admin@mealprep.com / Admin@123
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. Seed Roles
        var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var shipperRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        if (!await db.AppRoles.AnyAsync())
        {
            db.AppRoles.AddRange(
                new AppRole { Id = adminRoleId, Name = Role.Admin },
                new AppRole { Id = userRoleId, Name = Role.User },
                new AppRole { Id = shipperRoleId, Name = Role.Shipper }
            );
            await db.SaveChangesAsync();
        }
        else
        {
            // Make sure both roles exist even if seeding was partial
            if (!await db.AppRoles.AnyAsync(r => r.Name == Role.Admin))
            {
                db.AppRoles.Add(new AppRole { Id = adminRoleId, Name = Role.Admin });
                await db.SaveChangesAsync();
            }
            if (!await db.AppRoles.AnyAsync(r => r.Name == Role.User))
            {
                db.AppRoles.Add(new AppRole { Id = userRoleId, Name = Role.User });
                await db.SaveChangesAsync();
            }
            if (!await db.AppRoles.AnyAsync(r => r.Name == Role.Shipper))
            {
                db.AppRoles.Add(new AppRole { Id = shipperRoleId, Name = Role.Shipper });
                await db.SaveChangesAsync();
            }

            // Retrieve actual IDs from DB in case they differ
            var adminRole = await db.AppRoles.FirstAsync(r => r.Name == Role.Admin);
            var userRole = await db.AppRoles.FirstAsync(r => r.Name == Role.User);
            var shipperRole = await db.AppRoles.FirstAsync(r => r.Name == Role.Shipper);
            adminRoleId = adminRole.Id;
            userRoleId = userRole.Id;
            shipperRoleId = shipperRole.Id;
        }

        // 2. Seed Admin user
        if (!await db.Users.AnyAsync(u => u.AppRole.Name == Role.Admin))
        {
            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@mealprep.com",
                FullName = "System Admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                AppRoleId = adminRoleId,
                Age = 30,
                Gender = Gender.Unknown,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                Allergies = Array.Empty<string>()
            });
            await db.SaveChangesAsync();
            Console.WriteLine("[Seed] Admin account created → admin@mealprep.com / Admin@123");
        }

        // 3. Seed Shipper user
        if (!await db.Users.AnyAsync(u => u.AppRole.Name == Role.Shipper))
        {
            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = "shipper@mealprep.com",
                FullName = "Shipper Demo",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Shipper@123"),
                AppRoleId = shipperRoleId,
                Age = 25,
                Gender = Gender.Unknown,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                Allergies = Array.Empty<string>()
            });
            await db.SaveChangesAsync();
            Console.WriteLine("[Seed] Shipper account created → shipper@mealprep.com / Shipper@123");
        }

        // 4. Seed Plans
        if (!await db.Plans.AnyAsync())
        {
            db.Plans.AddRange(
                new Plan
                {
                    Name = "Gói Cơ Bản",
                    Description = "Phù hợp để bắt đầu hành trình ăn uống lành mạnh trong 1 tuần.",
                    DurationDays = 7,
                    MealsPerDay = 1,
                    BasePrice = 350_000m,
                    ExtraPrice = 80_000m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Plan
                {
                    Name = "Gói Tiêu Chuẩn",
                    Description = "Trải nghiệm 2 tuần với 2 bữa ăn dinh dưỡng mỗi ngày.",
                    DurationDays = 14,
                    MealsPerDay = 2,
                    BasePrice = 980_000m,
                    ExtraPrice = 80_000m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Plan
                {
                    Name = "Gói Cao Cấp",
                    Description = "Trọn gói 30 ngày — 3 bữa tươi ngon mỗi ngày, tối ưu cho mọi mục tiêu.",
                    DurationDays = 30,
                    MealsPerDay = 3,
                    BasePrice = 2_500_000m,
                    ExtraPrice = 80_000m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync();
            Console.WriteLine("[Seed] 3 subscription plans created.");
        }

        // 5. Seed Delivery Slots
        if (!await db.DeliverySlots.AnyAsync())
        {
            db.DeliverySlots.AddRange(
                new DeliverySlot
                {
                    Name = "Buổi sáng (6:30 - 8:30)",
                    Capacity = 120,
                    IsActive = true
                },
                new DeliverySlot
                {
                    Name = "Buổi trưa (11:00 - 13:00)",
                    Capacity = 150,
                    IsActive = true
                },
                new DeliverySlot
                {
                    Name = "Buổi tối (17:30 - 19:30)",
                    Capacity = 150,
                    IsActive = true
                }
            );
            await db.SaveChangesAsync();
            Console.WriteLine("[Seed] Default delivery slots created.");
        }

        // 6. Seed Meals (only if none exist)
        if (!await db.Meals.AnyAsync())
        {
            var now = DateTime.UtcNow;
            var meals = GetSeedMeals(now);
            db.Meals.AddRange(meals);
            await db.SaveChangesAsync();
            Console.WriteLine($"[Seed] {meals.Count} demo meals created.");
        }

        // 7. Backfill ingredient quantities for meals that still use plain ingredient names
        var existingMeals = await db.Meals.ToListAsync();
        var changedMeals = MealSeedData.ApplyDefaultIngredientQuantities(existingMeals);
        if (changedMeals > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[Seed] Added default ingredient quantities for {changedMeals} meals.");
        }

        // 8. Seed historical dataset (2023 -> today) for dashboard testing
        await SeedHistoricalLoadTestDataAsync(db, userRoleId, shipperRoleId);
    }

    private static List<Meal> GetSeedMeals(DateTime createdAt)
    {
        return MealSeedData.GetAllMeals(createdAt);
    }

    private static async Task SeedHistoricalLoadTestDataAsync(
        AppDbContext db,
        Guid userRoleId,
        Guid shipperRoleId
    )
    {
        var from = SeedStartDate;
        var to = DateOnly.FromDateTime(DateTime.UtcNow);
        var utcNow = DateTime.UtcNow;

        var existingOrders = await db.Orders.CountAsync(o => o.DeliveryDate >= from && o.DeliveryDate <= to);
        if (existingOrders >= 300)
        {
            Console.WriteLine($"[Seed] Historical dataset already exists ({existingOrders} orders in 2023->today). Skipped.");
            return;
        }

        var rnd = new Random(20260325);

        await EnsureTestUsersAsync(db, userRoleId, shipperRoleId, rnd);

        var customers = await db.Users
            .Where(u => u.AppRoleId == userRoleId && u.IsActive)
            .ToListAsync();
        var shippers = await db.Users
            .Where(u => u.AppRoleId == shipperRoleId && u.IsActive)
            .ToListAsync();
        var plans = await db.Plans.Where(p => p.IsActive).ToListAsync();
        var meals = await db.Meals.Where(m => m.IsActive).ToListAsync();
        var slots = await db.DeliverySlots.Where(s => s.IsActive).ToListAsync();

        if (!customers.Any() || !plans.Any() || !meals.Any() || !slots.Any())
        {
            Console.WriteLine("[Seed] Historical dataset skipped because required base data is missing.");
            return;
        }

        var subscriptions = new List<Subscription>();
        for (var year = from.Year; year <= to.Year; year++)
        {
            var monthStart = year == from.Year ? from.Month : 1;
            var monthEnd = year == to.Year ? to.Month : 12;
            for (var month = monthStart; month <= monthEnd; month++)
            {
                var monthlyCount = 6;
                for (var i = 0; i < monthlyCount; i++)
                {
                    var customer = customers[rnd.Next(customers.Count)];
                    var plan = plans[rnd.Next(plans.Count)];
                    var day = rnd.Next(1, DateTime.DaysInMonth(year, month) + 1);
                    var startDate = new DateOnly(year, month, day);
                    if (startDate > to)
                    {
                        continue;
                    }
                    var rawEnd = startDate.AddDays(plan.DurationDays - 1);
                    var endDate = rawEnd > to ? to : rawEnd;

                    var status = ResolveSubscriptionStatus(endDate, rnd);
                    var mealsPerDay = Math.Clamp(plan.MealsPerDay + rnd.Next(-1, 2), 1, 3);
                    var totalAmount = plan.BasePrice + (mealsPerDay - 1) * plan.ExtraPrice * plan.DurationDays;

                    subscriptions.Add(new Subscription
                    {
                        UserId = customer.Id,
                        PlanId = plan.Id,
                        CustomerName = customer.FullName,
                        CustomerEmail = customer.Email,
                        MealsPerDay = mealsPerDay,
                        DeliveryTimeSlot = rnd.NextDouble() < 0.33 ? "Morning" : rnd.NextDouble() < 0.66 ? "Afternoon" : "Evening",
                        StartDate = startDate,
                        EndDate = endDate,
                        Status = status,
                        TotalAmount = totalAmount,
                        CreatedAt = startDate.ToDateTime(new TimeOnly(8, rnd.Next(0, 59)), DateTimeKind.Utc),
                        UpdatedAt = startDate.ToDateTime(new TimeOnly(10, rnd.Next(0, 59)), DateTimeKind.Utc),
                    });
                }
            }
        }

        db.Subscriptions.AddRange(subscriptions);
        await db.SaveChangesAsync();

        var payments = new List<Payment>();
        var paymentTxs = new List<PaymentTransaction>();
        foreach (var sub in subscriptions)
        {
            var createdAt = sub.StartDate.ToDateTime(new TimeOnly(9, rnd.Next(0, 59)), DateTimeKind.Utc).AddDays(-rnd.Next(0, 3));
            var minCreatedAt = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            if (createdAt < minCreatedAt)
            {
                createdAt = minCreatedAt;
            }
            if (createdAt > utcNow)
            {
                createdAt = utcNow;
            }

            var method = PickOne(rnd, "MoMo", "VNPay", "BankTransfer", "COD");
            var paymentStatus = ResolvePaymentStatus(sub.Status, rnd);
            var paymentCode = $"SEED-{sub.Id}-{Guid.NewGuid():N}";
            var paidAt = paymentStatus == "Paid" ? createdAt.AddMinutes(rnd.Next(5, 120)) : (DateTime?)null;
            if (paidAt.HasValue && paidAt.Value > utcNow)
            {
                paidAt = utcNow;
            }

            var expiredAt = paymentStatus is "Expired" or "Pending" ? createdAt.AddDays(2) : (DateTime?)null;
            if (expiredAt.HasValue && expiredAt.Value > utcNow)
            {
                expiredAt = utcNow;
            }

            var payment = new Payment
            {
                UserId = sub.UserId,
                SubscriptionId = sub.Id,
                Amount = sub.TotalAmount,
                Currency = "VND",
                Method = method,
                Status = paymentStatus,
                PaymentCode = paymentCode,
                Description = $"Seed payment for subscription {sub.Id}",
                CreatedAt = createdAt,
                PaidAt = paidAt,
                ExpiredAt = expiredAt,
            };

            payments.Add(payment);
        }

        db.Payments.AddRange(payments);
        await db.SaveChangesAsync();

        foreach (var payment in payments)
        {
            paymentTxs.Add(new PaymentTransaction
            {
                PaymentId = payment.Id,
                Gateway = payment.Method,
                RequestId = $"REQ-{Guid.NewGuid():N}"[..20],
                OrderId = $"ORD-{payment.Id}",
                ResponseCode = payment.Status == "Paid" ? "0" : "99",
                ResponseMessage = payment.Status == "Paid" ? "Success" : payment.Status,
                RawResponseJson = "{}",
                CreatedAt = payment.PaidAt ?? payment.CreatedAt,
            });
        }

        db.PaymentTransactions.AddRange(paymentTxs);
        await db.SaveChangesAsync();

        var orders = new List<Order>();
        foreach (var sub in subscriptions)
        {
            if (!sub.EndDate.HasValue || sub.EndDate.Value < from)
                continue;

            var startDate = sub.StartDate < from ? from : sub.StartDate;
            var endDate = sub.EndDate.Value > to ? to : sub.EndDate.Value;
            var span = endDate.DayNumber - startDate.DayNumber + 1;
            if (span <= 0)
                continue;

            var orderCount = Math.Clamp(span / 7 + rnd.Next(1, 3), 1, 10);
            for (var i = 0; i < orderCount; i++)
            {
                var deliveryDate = startDate.AddDays(rnd.Next(0, span));
                var status = ResolveOrderStatus(deliveryDate, rnd);
                Guid? shipperId = null;
                if (shippers.Any() && status is OrderStatus.Delivering or OrderStatus.Delivered or OrderStatus.ConfirmedByUser or OrderStatus.Completed or OrderStatus.InProgress)
                {
                    shipperId = shippers[rnd.Next(shippers.Count)].Id;
                }

                orders.Add(new Order
                {
                    UserId = sub.UserId,
                    SubscriptionId = sub.Id,
                    DeliveryDate = deliveryDate,
                    ShipperId = shipperId,
                    Status = status,
                    IsAutoFilled = rnd.NextDouble() < 0.12,
                });
            }
        }

        db.Orders.AddRange(orders);
        await db.SaveChangesAsync();

        var orderItems = new List<OrderItem>();
        foreach (var order in orders)
        {
                var itemCount = rnd.Next(1, 3);
            for (var i = 0; i < itemCount; i++)
            {
                var meal = meals[rnd.Next(meals.Count)];
                var slot = slots[rnd.Next(slots.Count)];
                var itemStatus = ResolveOrderItemStatus(order.Status);

                orderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    MealId = meal.Id,
                    Status = itemStatus,
                    Quantity = rnd.Next(1, 3),
                    DeliverySlotId = slot.Id,
                    DeliveryAddress = $"{rnd.Next(1, 300)} Nguyen Trai, Quan {rnd.Next(1, 12)}, TP.HCM",
                    DeliveredAt = itemStatus == OrderItemStatus.Delivered
                        ? order.DeliveryDate.ToDateTime(new TimeOnly(rnd.Next(7, 20), rnd.Next(0, 60)), DateTimeKind.Utc)
                        : null,
                });
            }
        }

        db.OrderItems.AddRange(orderItems);
        await db.SaveChangesAsync();

        var ratings = new List<MealRating>();
        var nowDate = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var item in orderItems)
        {
            var order = orders.First(o => o.Id == item.OrderId);
            if (order.DeliveryDate >= nowDate.AddDays(-1))
                continue;
            if (item.Status != OrderItemStatus.Delivered)
                continue;
            if (rnd.NextDouble() > 0.42)
                continue;

            var stars = ResolveStars(rnd);
            ratings.Add(new MealRating
            {
                UserId = order.UserId,
                OrderItemId = item.Id,
                MealId = item.MealId,
                DeliveryDate = order.DeliveryDate,
                Stars = stars,
                Tags = stars >= 4
                    ? new[] { "Ngon", "Vua mieng" }
                    : stars <= 2
                        ? new[] { "Can cai thien" }
                        : new[] { "Tam on" },
                Comments = stars >= 4
                    ? "Hai long voi chat luong mon an."
                    : stars <= 2
                        ? "Can dieu chinh huong vi va nhiet do giao hang."
                        : "Trai nghiem trung binh.",
                CreatedAt = order.DeliveryDate.ToDateTime(new TimeOnly(rnd.Next(12, 22), rnd.Next(0, 60)), DateTimeKind.Utc),
            });
        }

        db.MealRatings.AddRange(ratings);
        await db.SaveChangesAsync();

        Console.WriteLine($"[Seed] Historical 2023->today dataset created: {subscriptions.Count} subscriptions, {payments.Count} payments, {orders.Count} orders, {orderItems.Count} items, {ratings.Count} ratings.");
    }

    private static async Task EnsureTestUsersAsync(
        AppDbContext db,
        Guid userRoleId,
        Guid shipperRoleId,
        Random rnd
    )
    {
        var fromUtc = SeedStartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = DateTime.UtcNow;

        var existingCustomerCount = await db.Users.CountAsync(u => u.AppRoleId == userRoleId);
        var targetCustomers = 24;
        if (existingCustomerCount < targetCustomers)
        {
            var toCreate = targetCustomers - existingCustomerCount;
            var users = new List<User>(toCreate);
            for (var i = 0; i < toCreate; i++)
            {
                var idx = existingCustomerCount + i + 1;
                var gender = rnd.NextDouble() < 0.48 ? Gender.Male : rnd.NextDouble() < 0.96 ? Gender.Female : Gender.Unknown;
                users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Email = $"customer{idx:D3}@mealprep.local",
                    FullName = BuildRealisticFullName(rnd, gender),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                    Gender = gender,
                    Age = rnd.Next(20, 46),
                    HeightCm = rnd.Next(155, 186),
                    WeightKg = rnd.Next(48, 95),
                    AppRoleId = userRoleId,
                    IsActive = true,
                    CreatedAtUtc = RandomUtcBetween(fromUtc, toUtc, rnd),
                    Goal = PickOne(rnd, FitnessGoal.FatLoss, FitnessGoal.Maintain, FitnessGoal.MuscleGain),
                    ActivityLevel = PickOne(rnd, ActivityLevel.LightlyActive, ActivityLevel.ModeratelyActive, ActivityLevel.VeryActive),
                    DietPreference = PickOne(rnd, DietPreference.None, DietPreference.Vegan, DietPreference.Vegetarian, DietPreference.Keto, DietPreference.LowCarb),
                    Allergies = Array.Empty<string>(),
                });
            }

            db.Users.AddRange(users);
            await db.SaveChangesAsync();
            Console.WriteLine($"[Seed] Added {toCreate} seeded customers.");
        }

        var existingShippers = await db.Users.CountAsync(u => u.AppRoleId == shipperRoleId);
        var targetShippers = 4;
        if (existingShippers < targetShippers)
        {
            var toCreate = targetShippers - existingShippers;
            var shippers = new List<User>(toCreate);
            for (var i = 0; i < toCreate; i++)
            {
                var idx = existingShippers + i + 1;
                var gender = rnd.NextDouble() < 0.7 ? Gender.Male : Gender.Female;
                shippers.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Email = $"shipper{idx:D3}@mealprep.local",
                    FullName = BuildRealisticFullName(rnd, gender),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Shipper@123"),
                    Gender = gender,
                    Age = rnd.Next(22, 40),
                    HeightCm = rnd.Next(160, 186),
                    WeightKg = rnd.Next(55, 90),
                    AppRoleId = shipperRoleId,
                    IsActive = true,
                    CreatedAtUtc = RandomUtcBetween(fromUtc, toUtc, rnd),
                    Goal = FitnessGoal.Maintain,
                    ActivityLevel = ActivityLevel.VeryActive,
                    DietPreference = DietPreference.None,
                    Allergies = Array.Empty<string>(),
                });
            }

            db.Users.AddRange(shippers);
            await db.SaveChangesAsync();
            Console.WriteLine($"[Seed] Added {toCreate} seeded shippers.");
        }
    }

    private static string BuildRealisticFullName(Random rnd, Gender gender)
    {
        var familyNames = new[] { "Nguyen", "Tran", "Le", "Pham", "Hoang", "Phan", "Vu", "Dang", "Bui", "Do" };
        var middleNames = new[] { "Minh", "Thanh", "Ngoc", "Anh", "Duc", "Quoc", "Khanh", "Gia", "Thu", "Bao" };
        var maleGivenNames = new[] { "Tuan", "Huy", "Kiet", "Phuc", "Long", "Nam", "Son", "Dat", "Khoa", "Hai" };
        var femaleGivenNames = new[] { "Linh", "My", "Thao", "Nhi", "Trang", "Yen", "Huong", "Han", "Vy", "Chi" };
        var neutralGivenNames = new[] { "An", "Ngan", "Binh", "Lam", "Quynh" };

        var family = familyNames[rnd.Next(familyNames.Length)];
        var middle = middleNames[rnd.Next(middleNames.Length)];
        string given;

        if (gender == Gender.Male)
        {
            given = maleGivenNames[rnd.Next(maleGivenNames.Length)];
        }
        else if (gender == Gender.Female)
        {
            given = femaleGivenNames[rnd.Next(femaleGivenNames.Length)];
        }
        else
        {
            given = neutralGivenNames[rnd.Next(neutralGivenNames.Length)];
        }

        return $"{family} {middle} {given}";
    }

    private static DateTime RandomUtcBetween(DateTime fromUtc, DateTime toUtc, Random rnd)
    {
        if (toUtc <= fromUtc)
        {
            return fromUtc;
        }

        var spanDays = (toUtc.Date - fromUtc.Date).Days;
        var dayOffset = rnd.Next(0, spanDays + 1);
        var date = fromUtc.Date.AddDays(dayOffset);
        var hour = rnd.Next(7, 21);
        var minute = rnd.Next(0, 60);
        var second = rnd.Next(0, 60);
        var candidate = new DateTime(date.Year, date.Month, date.Day, hour, minute, second, DateTimeKind.Utc);
        return candidate > toUtc ? toUtc : candidate;
    }

    private static SubscriptionStatus ResolveSubscriptionStatus(DateOnly endDate, Random rnd)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (endDate < today.AddDays(-2))
        {
            return rnd.NextDouble() < 0.9 ? SubscriptionStatus.Expired : SubscriptionStatus.Cancelled;
        }

        var roll = rnd.NextDouble();
        if (roll < 0.75)
            return SubscriptionStatus.Active;
        if (roll < 0.85)
            return SubscriptionStatus.Paused;
        if (roll < 0.94)
            return SubscriptionStatus.PendingPayment;
        return SubscriptionStatus.Cancelled;
    }

    private static string ResolvePaymentStatus(SubscriptionStatus subscriptionStatus, Random rnd)
    {
        if (subscriptionStatus == SubscriptionStatus.PendingPayment)
        {
            return rnd.NextDouble() < 0.65 ? "Pending" : "Expired";
        }

        var roll = rnd.NextDouble();
        if (roll < 0.82)
            return "Paid";
        if (roll < 0.9)
            return "Failed";
        if (roll < 0.96)
            return "Cancelled";
        return "Expired";
    }

    private static OrderStatus ResolveOrderStatus(DateOnly deliveryDate, Random rnd)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (deliveryDate > today)
        {
            var futureRoll = rnd.NextDouble();
            if (futureRoll < 0.45)
                return OrderStatus.Planned;
            if (futureRoll < 0.7)
                return OrderStatus.Preparing;
            if (futureRoll < 0.9)
                return OrderStatus.InProgress;
            return OrderStatus.Cancelled;
        }

        var roll = rnd.NextDouble();
        if (roll < 0.36)
            return OrderStatus.ConfirmedByUser;
        if (roll < 0.58)
            return OrderStatus.Delivered;
        if (roll < 0.66)
            return OrderStatus.Completed;
        if (roll < 0.74)
            return OrderStatus.Delivering;
        if (roll < 0.82)
            return OrderStatus.Preparing;
        if (roll < 0.92)
            return OrderStatus.Cancelled;
        return OrderStatus.Disputed;
    }

    private static OrderItemStatus ResolveOrderItemStatus(OrderStatus orderStatus)
    {
        return orderStatus switch
        {
            OrderStatus.Preparing => OrderItemStatus.Preparing,
            OrderStatus.Delivering or OrderStatus.InProgress => OrderItemStatus.Delivering,
            OrderStatus.Delivered or OrderStatus.ConfirmedByUser or OrderStatus.Completed => OrderItemStatus.Delivered,
            OrderStatus.Cancelled => OrderItemStatus.Cancelled,
            _ => OrderItemStatus.Planned,
        };
    }

    private static int ResolveStars(Random rnd)
    {
        var roll = rnd.NextDouble();
        if (roll < 0.07)
            return 1;
        if (roll < 0.18)
            return 2;
        if (roll < 0.4)
            return 3;
        if (roll < 0.72)
            return 4;
        return 5;
    }

    private static T PickOne<T>(Random rnd, params T[] values)
    {
        return values[rnd.Next(values.Length)];
    }
}
