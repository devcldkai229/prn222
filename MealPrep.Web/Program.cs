using Amazon;
using Amazon.S3;
using MealPrep.BLL.Extensions;
using MealPrep.DAL.Extensions;
using MealPrep.BLL.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Khi chạy sau ngrok, cần nhận X-Forwarded-* headers để URL/scheme đúng
builder.Services.Configure<ForwardedHeadersOptions>(opts =>
{
        opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    opts.KnownNetworks.Clear();
    opts.KnownProxies.Clear();
});

// Support both MVC controllers (existing) and Razor Pages (new)
builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.AddDalServices(builder.Configuration);

// Configure AWS S3 Client
var awsRegion = builder.Configuration["AwsS3:Region"] ?? "ap-northeast-1";
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(awsRegion) };
    return new AmazonS3Client(config);
});

builder.Services.AddBllServices();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Auth/Login";
        o.AccessDeniedPath = "/Auth/AccessDenied";
    });

var app = builder.Build();

app.UseForwardedHeaders();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// MVC routes (existing controllers)
app.MapControllerRoute(name: "default", pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

// Razor Pages routes (new Auth + Admin pages)
app.MapRazorPages();

// SignalR hub endpoints
app.MapHub<OrderHub>("/hubs/order");
app.MapHub<OrderTrackingHub>("/hubs/order-tracking");
app.MapHub<DashboardHub>("/hubs/dashboard");
app.MapHub<MealHub>("/hubs/meal");

// Redirect root "/" to landing/home page
app.MapGet("/", () => Results.Redirect("/home"));

// Seed default roles + admin account on first run
await MealPrep.DAL.Data.DatabaseSeeder.SeedAsync(app.Services);

app.Run();
