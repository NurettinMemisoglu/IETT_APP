using IETT_APP.Infrastructure.Converters;
using IETT_APP.WebMVC.Services.Implementations;
using IETT_APP.WebMVC.Services.Infrastructure;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ==================================================================================
// 1. TEMEL AYARLAR (Culture & HttpContext)
// ==================================================================================
var defaultCulture = new CultureInfo("en-US");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Tarih formatı dönüştürücüsünü ekliyoruz
        options.JsonSerializerOptions.Converters.Add(new TrDateTimeConverter());

        // (Opsiyonel) Property isimlerini olduğu gibi koru (camelCase yapma)
        // options.JsonSerializerOptions.PropertyNamingPolicy = null; 
    });

// ==================================================================================
// 2. SESSION & CACHE
// ==================================================================================
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ==================================================================================
// 3. AUTHENTICATION (COOKIE)
// ==================================================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.Name = "IETT_Session";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

// ==================================================================================
// 4. API SERVİSLERİ VE HTTPCLIENT YAPILANDIRMASI
// ==================================================================================

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                 ?? throw new InvalidOperationException("ApiSettings:BaseUrl is missing in appsettings.json");

// Token Handler'ı DI Container'a ekle
builder.Services.AddTransient<JwtDelegatingHandler>();

// Ortak HttpClient Ayarı
void ConfigureApi(HttpClient client)
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
}

// --- Servis Kayıtları ---

builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(ConfigureApi)
    .AddHttpMessageHandler<JwtDelegatingHandler>();

builder.Services.AddHttpClient<IApiUserService, ApiUserService>(ConfigureApi)
    .AddHttpMessageHandler<JwtDelegatingHandler>();

builder.Services.AddHttpClient<IDriverApiService, DriverApiService>(ConfigureApi)
    .AddHttpMessageHandler<JwtDelegatingHandler>();

builder.Services.AddHttpClient<IStopApiService, StopApiService>(ConfigureApi)
    .AddHttpMessageHandler<JwtDelegatingHandler>();

builder.Services.AddHttpClient<ILineApiService, LineApiService>(ConfigureApi)
    .AddHttpMessageHandler<JwtDelegatingHandler>();

builder.Services.AddHttpClient<IRouteApiService, RouteApiService>(ConfigureApi)
    .AddHttpMessageHandler<JwtDelegatingHandler>();

builder.Services.AddHttpClient<IVehicleApiService, VehicleApiService>(ConfigureApi)
    .AddHttpMessageHandler<JwtDelegatingHandler>();

builder.Services.AddHttpClient<IGarageApiService, GarageApiService>(ConfigureApi)
    .AddHttpMessageHandler<JwtDelegatingHandler>();

builder.Services.AddHttpClient<ITripTaskApiService, TripTaskApiService>(ConfigureApi)
    .AddHttpMessageHandler<JwtDelegatingHandler>();

builder.Services.AddHttpClient<INotificationApiService, NotificationApiService>(ConfigureApi)
    .AddHttpMessageHandler<JwtDelegatingHandler>();

// 3. Helper Servisler (Scoped)
builder.Services.AddScoped<IUserSessionApiService, UserSessionApiService>();


// ==================================================================================
// 5. MIDDLEWARE PIPELINE
// ==================================================================================
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization(localizationOptions);
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Route Mappings
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"); // Açılışta Login

app.Run();