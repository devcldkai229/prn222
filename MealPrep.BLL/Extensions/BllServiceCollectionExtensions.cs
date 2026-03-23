using Amazon.S3;
using MealPrep.BLL.Services;
using MealPrep.BLL.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MealPrep.BLL.Extensions
{
    public static class BllServiceCollectionExtensions
    {
        public static IServiceCollection AddBllServices(this IServiceCollection services)
        {
            // ── Core Auth & Email ────────────────────────────────────────────────
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();

            // ── Admin Services ───────────────────────────────────────────────────
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IAdminDeliveryOrderService, AdminDeliveryOrderService>();
            services.AddScoped<IDeliveryProcessingService, DeliveryProcessingService>();

            // ── AI Menu ──────────────────────────────────────────────────────────
            services.AddScoped<IAiMenuService, AiMenuService>();

            // ── AWS S3 ───────────────────────────────────────────────────────────
            services.AddScoped<IS3Service, S3Service>();

            // ── HTTP / Payment ───────────────────────────────────────────────────
            services.AddHttpClient();
            services.AddScoped<IMomoService>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();
                var configuration = sp.GetRequiredService<IConfiguration>();
                var logger = sp.GetRequiredService<ILogger<MomoPaymentService>>();
                return new MomoPaymentService(configuration, httpClient, logger);
            });

            // ── User & Nutrition ─────────────────────────────────────────────────
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<INutritionLogService, NutritionLogService>();

            // ── Subscription & Payment ───────────────────────────────────────────
            services.AddScoped<ISubscriptionService, SubscriptionService>();

            // ── Menu & Feedback ──────────────────────────────────────────────────
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IMealFeedbackService, MealFeedbackService>();

            // ── Shipper ──────────────────────────────────────────────────────────
            services.AddScoped<IShipperService, ShipperService>();

            // ── Module 7: Admin Catalog ──────────────────────────────────────────
            services.AddScoped<IMealService, MealService>();
            services.AddScoped<IPlanService, PlanService>();

            // ── Module 8: Admin Operations ───────────────────────────────────────
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAdminSubscriptionService, AdminSubscriptionService>();

            // ── Module 9: Dashboard & Catalog ────────────────────────────────────
            services.AddScoped<IDashboardService, DashboardService>();

            // ── Module 10: Order Tracking & Support ──────────────────────────────
            services.AddScoped<IOrderTrackingService, OrderTrackingService>();
            services.AddScoped<IAdminSupportService, AdminSupportService>();

            return services;
        }
    }
}
