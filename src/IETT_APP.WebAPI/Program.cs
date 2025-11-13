using IETT_APP.Application.Interfaces;
using IETT_APP.Application.Interfaces.Garages;
using IETT_APP.Application.Mapping;
using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using IETT_APP.Domain.Services;
using IETT_APP.Infrastructure.Persistence;
using IETT_APP.Infrastructure.Persistence.Interceptors;
using IETT_APP.Infrastructure.Persistence.Repositories;
using IETT_APP.Infrastructure.Persistence.Seed;
using IETT_APP.Infrastructure.Services;
using IETT_APP.WebAPI.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Globalization;
using System.Text;

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

// ✅ Global Culture Fix – her zaman nokta (.) kullan
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

// ----------------------
// Timezone Fix (UTC+3 Türkiye Saati)
// ----------------------
TimeZoneInfo turkeyZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
TimeZoneInfo.ClearCachedData();

// Tüm DateTime kayıtlarını Türkiye saatine göre ayarlamak için helper
DateTime nowTurkey = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyZone);

// Uygulama genelinde DateTime.Now Türkiye saatine eşit olacak şekilde
AppContext.SetSwitch("System.Globalization.UseNls", true);
DateTime.SpecifyKind(nowTurkey, DateTimeKind.Local);

// ----------------------
// Database & Identity
// ----------------------
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ----------------------
// JWT Authentication
// ----------------------
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
});

// ----------------------
// Dependency Injection
// ----------------------
// Service Line
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStopService, StopService>();
builder.Services.AddScoped<IRouteService<Guid>, RouteService<Guid>>();
builder.Services.AddScoped<ILineService<Guid>, LineService<Guid>>();
builder.Services.AddScoped<IVehicleService<Guid>, VehicleService<Guid>>();
builder.Services.AddScoped<IGarageService, GarageService>();
builder.Services.AddScoped<ITripTaskService, TripTaskService>();
builder.Services.AddScoped<TripTaskDomainService>();

builder.Services.AddScoped<AuditInterceptor>();
// Repository Line
builder.Services.AddScoped<IRouteRepository<Guid>, RouteRepository<Guid>>();
builder.Services.AddScoped<ILineRepository<Guid>, LineRepository<Guid>>();
builder.Services.AddScoped<IVehicleRepository<Guid>, VehicleRepository<Guid>>();
builder.Services.AddScoped<ITripTaskRepository, TripTaskRepository>();
//AutoMapper Line
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<LineProfile>();
    cfg.AddProfile<RouteProfile>();
    cfg.AddProfile<VehicleProfile>();
    cfg.AddProfile<TripTaskProfile>();
});

// ----------------------
// CORS
// ----------------------
var mvcOrigin = builder.Configuration["MvcClient:Url"] ?? "https://localhost:5001";
builder.Services.AddCors(options =>
{
    options.AddPolicy("MvcClient", p =>
        p.WithOrigins(mvcOrigin)
         .AllowAnyHeader()
         .AllowAnyMethod());
});

// ----------------------
// Controllers & OpenAPI
// ----------------------
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ----------------------
// Middleware Pipeline
// ----------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// CORS – controllerlardan önce olmalı
app.UseCors("MvcClient");

// Custom middleware
app.UseCustomMiddleware();

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Localization (virgül-nokta davranışı için)
app.UseRequestLocalization(localizationOptions);

// ----------------------
// Database Seed
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeed.SeedAdminAsync(services);
    await IdentitySeed.SeedPlannerAsync(services);
    await IdentitySeed.SeedChiefAsync(services);
    await IdentitySeed.SeedDriverAsync(services);
    await IdentitySeed.SeedUserAsync(services);
    await GarageSeed.SeedGaragesAsync(services);
}

// ----------------------
// Controllers
// ----------------------
app.MapControllers();

// ----------------------
// Run
// ----------------------
app.Run();
