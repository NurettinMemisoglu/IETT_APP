

using IETT_APP.Domain.Entities;
using IETT_APP.Infrastructure.Persistence;
using IETT_APP.WebMVC.Services.Implementations;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Localization (EN culture)
// ----------------------
var defaultCulture = new CultureInfo("en-US");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

// ----------------------
// Database
// ----------------------
var connectionString = builder.Configuration.GetConnectionString("AppDbContextConnection")
                       ?? throw new InvalidOperationException("Connection string 'AppDbContextConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ----------------------
// Identity
// ----------------------
builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Required for Identity UI

// ----------------------
// Session & Caching
// ----------------------
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// ----------------------
// HttpContext
// ----------------------
builder.Services.AddHttpContextAccessor();

// ----------------------
// API Servisleri
// ----------------------
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                 ?? throw new InvalidOperationException("ApiSettings:BaseUrl is missing.");


builder.Services.AddHttpClient<IApiUserService, ApiUserService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IStopApiService, StopApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<ILineApiService, LineApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IRouteApiService, RouteApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// ----------------------
// Cookie & Authentication
// ----------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// ----------------------
// Build App
// ----------------------

// ✅ Global Culture Fix – decimal’ler artık her zaman “.” kullanacak
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization(localizationOptions);
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ----------------------
// Route Mappings
// ----------------------
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Enable Identity UI pages

app.Run();
