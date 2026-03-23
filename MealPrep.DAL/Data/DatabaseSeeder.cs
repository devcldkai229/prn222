using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MealPrep.DAL.Data;

public static class DatabaseSeeder
{
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
    }

    private static List<Meal> GetSeedMeals(DateTime createdAt)
    {
        return MealSeedData.GetAllMeals(createdAt);
    }
}
