using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<AppRole> AppRoles => Set<AppRole>();
        public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
        public DbSet<Meal> Meals => Set<Meal>();
        public DbSet<WeeklyMenu> WeeklyMenus => Set<WeeklyMenu>();
        public DbSet<WeeklyMenuItem> WeeklyMenuItems => Set<WeeklyMenuItem>();
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<DeliverySlot> DeliverySlots => Set<DeliverySlot>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<NutritionLog> NutritionLogs => Set<NutritionLog>();
        public DbSet<MealRating> MealRatings => Set<MealRating>();
        public DbSet<KitchenInventory> KitchenInventories => Set<KitchenInventory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình bảng Meal (Embedding dùng double precision[] - không cần pgvector)
            modelBuilder.Entity<Meal>(entity =>
            {
                entity.Property(e => e.Embedding)
                      .HasColumnType("double precision[]");
            });

            // 2. Cấu hình quan hệ Many-to-Many: User - DislikedMeals
            // Tự động tạo một bảng trung gian tên là "UserDislikedMeals" trong Database
            modelBuilder.Entity<User>()
                .HasMany(u => u.DislikedMeals)
                .WithMany()
                .UsingEntity(j => j.ToTable("UserDislikedMeals"));

            // 3. Cấu hình Behavior để tránh lỗi Multiple Cascade Paths (Xóa dữ liệu dây chuyền)

            // Order trỏ về User bằng 2 đường (Customer và Shipper)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Tránh vô tình xóa User làm mất Order

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Shipper)
                .WithMany()
                .HasForeignKey(o => o.ShipperId)
                .OnDelete(DeleteBehavior.SetNull); // Nếu Shipper nghỉ việc, giữ lại Order nhưng ShipperId = null

            // MealRating trỏ về User và Meal (dễ dính vòng lặp xóa)
            modelBuilder.Entity<MealRating>()
                .HasOne(mr => mr.User)
                .WithMany()
                .HasForeignKey(mr => mr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MealRating>()
                .HasOne(mr => mr.Meal)
                .WithMany()
                .HasForeignKey(mr => mr.MealId)
                .OnDelete(DeleteBehavior.Restrict);

            // Các quan hệ cha - con chặt chẽ (Xóa cha thì con mất)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(pt => pt.Payment)
                .WithMany(p => p.Transactions)
                .HasForeignKey(pt => pt.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. OrderItem.ImageS3Keys — map string[] to PostgreSQL native text[] array
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(e => e.ImageS3Keys)
                    .HasColumnType("text[]");
            });

            // 5. KitchenInventory - Unique constraint: Date + MealId
            modelBuilder.Entity<KitchenInventory>(entity =>
            {
                entity.HasIndex(e => new { e.Date, e.MealId }).IsUnique();
                entity.HasOne(e => e.Meal).WithMany().HasForeignKey(e => e.MealId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}