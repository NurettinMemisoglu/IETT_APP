using Hangfire;
using IETT_APP.Application.Interfaces;
using IETT_APP.Application.Interfaces.Garages;
using IETT_APP.Application.Mapping;
using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using IETT_APP.Domain.Services;
using IETT_APP.Infrastructure.Hubs;
using IETT_APP.Infrastructure.Persistence;
using IETT_APP.Infrastructure.Persistence.Interceptors;
using IETT_APP.Infrastructure.Persistence.Repositories;
using IETT_APP.Infrastructure.Persistence.Seed;
using IETT_APP.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Gerekli
using Scalar.AspNetCore;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==================================================================================
// 1. GLOBAL CONFIGURATIONS (Culture & Settings)
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

// ==================================================================================
// 2. INFRASTRUCTURE SETUP (DB, Identity, Auth)
// ==================================================================================
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddScoped<TripTaskHistoryInterceptor>();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
    var historyInterceptor = sp.GetRequiredService<TripTaskHistoryInterceptor>();
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            .AddInterceptors(auditInterceptor, historyInterceptor);
});

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// --- JWT Authentication ---
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "SuperSecretKey1234567890_SetInAppSettings");
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

    // ====================================================================
    // 🛠️ SİGNALR İÇİN EKLENMESİ GEREKEN KISIM (TOKEN'I QUERY'DEN OKUMA)
    // ====================================================================
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // İstemciden gelen token'ı al
            var accessToken = context.Request.Query["access_token"];

            // Eğer istek SignalR Hub'ına gidiyorsa ve token varsa
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                // Token'ı context'e ata (Böylece Authorize attribute'u çalışır)
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// ==================================================================================
// 3. DEPENDENCY INJECTION (Services & Repositories)
// ==================================================================================

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped(typeof(IRouteRepository<>), typeof(RouteRepository<>));
builder.Services.AddScoped(typeof(ILineRepository<>), typeof(LineRepository<>));
builder.Services.AddScoped(typeof(IVehicleRepository<>), typeof(VehicleRepository<>));
builder.Services.AddScoped<ITripTaskRepository, TripTaskRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
builder.Services.AddScoped<IGarageRepository, GarageRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

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
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IOperationJobService, OperationJobService>();

builder.Services.AddScoped<DriverDomainService>();
builder.Services.AddScoped<TripTaskDomainService>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<LineProfile>();
    cfg.AddProfile<RouteProfile>();
    cfg.AddProfile<VehicleProfile>();
    cfg.AddProfile<TripTaskProfile>();
    cfg.AddProfile<UserProfile>();
    cfg.AddProfile<DriverProfile>();
    cfg.AddProfile<NotificationProfile>();
});


// ==================================================================================
// 4. API CONFIGURATION (CORS, Controllers, OpenAPI)
// ==================================================================================

var mvcOrigin = builder.Configuration["MvcClient:Url"] ?? "https://localhost:5001";
builder.Services.AddCors(options =>
{
    options.AddPolicy("MvcClient", p =>
        p.WithOrigins(mvcOrigin)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

builder.Services.AddControllers()
    // Sonsuz döngü (Circular Reference) hatasını çözmek için:
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddMemoryCache();

// 🔥 GÜNCELLENMİŞ VE BİRLEŞTİRİLMİŞ OPENAPI AYARI 🔥
builder.Services.AddOpenApi(options =>
{
    // 1. Genel Bilgiler ve JWT
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "IETT API",
            Version = "v1",
            Description = "IETT Operasyon Yönetim Sistemi API"
        };

        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Token'ı buraya yapıştırın."
        };

        document.Components ??= new OpenApiComponents();
        if (!document.Components.SecuritySchemes.ContainsKey("Bearer"))
        {
            document.Components.SecuritySchemes.Add("Bearer", securityScheme);
        }

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                new List<string>()
            }
        });

        return Task.CompletedTask;
    });

    // 2. SCALAR DOSYA YÜKLEME DÜZELTMESİ (TEK BLOK)
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var path = context.Description.RelativePath;
        if (string.IsNullOrEmpty(path)) return Task.CompletedTask;

        // SENARYO A: Profil Fotoğrafı Yükleme (upload-photo)
        if (path.Contains("upload-photo", StringComparison.OrdinalIgnoreCase))
        {
            if (operation.RequestBody?.Content.ContainsKey("multipart/form-data") == true)
            {
                var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

                // Temizle ve yeniden oluştur
                schema.Properties.Clear();
                schema.Type = "object";

                schema.Properties.Add("photo", new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = "Yüklenecek resim dosyası"
                });

                schema.Required = new HashSet<string> { "photo" };

                // Diğer content tiplerini temizle
                operation.RequestBody.Content.Clear();
                operation.RequestBody.Content.Add("multipart/form-data", new OpenApiMediaType { Schema = schema });
            }
        }

        // SENARYO B: Profil Tamamlama (complete-profile)
        else if (path.Contains("complete-profile", StringComparison.OrdinalIgnoreCase))
        {
            if (operation.RequestBody?.Content.ContainsKey("multipart/form-data") == true)
            {
                var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

                // Temizle ve yeniden oluştur
                schema.Properties.Clear();
                schema.Type = "object";

                // 1. JSON Verisi (String)
                schema.Properties.Add("data", new OpenApiSchema
                {
                    Type = "string",
                    Format = "", // Dosya sanmasın diye boş
                    Description = "CompleteProfileDto JSON verisi",
                    Example = new Microsoft.OpenApi.Any.OpenApiString("{\"EmployeeNumber\": \"12345\"}")
                });

                // 2. Dosyaları (Binary)
                schema.Properties.Add("licenseDocument", new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = "Ehliyet Belgesi"
                });

                schema.Properties.Add("psychotechnicDocument", new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = "Psikoteknik Belgesi"
                });

                schema.Required = new HashSet<string> { "data" };

                // Diğer content tiplerini temizle
                operation.RequestBody.Content.Clear();
                operation.RequestBody.Content.Add("multipart/form-data", new OpenApiMediaType { Schema = schema });
            }
        }

        return Task.CompletedTask;
    });
});

// 2. HANGFIRE SQL AYARLARI
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"))); // ConnectionString'in adı

// Hangfire Sunucusunu Ekle
builder.Services.AddHangfireServer();

var app = builder.Build();

// ==================================================================================
// 5. MIDDLEWARE PIPELINE
// ==================================================================================

app.UseMiddleware<IETT_APP.WebAPI.Middlewares.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
// HANGFIRE DASHBOARD (GÜVENLİ)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "İETT Operasyon Paneli", // Tarayıcı sekmesinde görünen isim

    // (Opsiyonel) ReadOnly modu: True yaparsan panelden iş tetiklenemez, sadece izlenir.
    // Şimdilik False (kapalı) bırakıyorum ki işleri yönetebilesin.
    IsReadOnlyFunc = context => false
});
app.UseRequestLocalization(localizationOptions);
app.UseHttpsRedirection();
app.UseStaticFiles();

// KESİN KURAL: UseCors, Auth'dan ÖNCE gelmeli
app.UseCors("MvcClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification"); // Endpoint

// ==================================================================================
// 6. DATABASE SEEDING
// ==================================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await IdentitySeed.SeedAsync(services);
        await GarageSeed.SeedGaragesAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // Ortak TimeZone ayarı (Her seferinde new'lememek için)
    var trTimeZone = new RecurringJobOptions { TimeZone = TimeZoneInfo.Local };

    // A) GECİKME KONTROLÜ (Her Dakika)
    recurringJobManager.AddOrUpdate<IOperationJobService>(
        "check-delayed-trips",
        service => service.CheckDelayedTripsAsync(),
        Cron.Minutely,
        trTimeZone // YENİ KULLANIM
    );

    // B) VARDİYA KAPANIŞI (Her gece 03:00)
    recurringJobManager.AddOrUpdate<IOperationJobService>(
        "auto-close-shift",
        service => service.AutoCloseShiftAsync(),
        Cron.Daily(3, 0),
        trTimeZone // YENİ KULLANIM
    );

    // C) MUAYENE VE EHLİYET KONTROLÜ (Her sabah 08:30)
    recurringJobManager.AddOrUpdate<IOperationJobService>(
        "check-vehicle-expirations",
        service => service.CheckExpirationsAsync(),
        Cron.Daily(8, 30),
        trTimeZone // YENİ KULLANIM
    );

    // D) HAFTALIK PERFORMANS RAPORU (Her Pazartesi 09:00)
    recurringJobManager.AddOrUpdate<IOperationJobService>(
        "weekly-performance-report",
        service => service.SendWeeklyReportAsync(),
        Cron.Weekly(DayOfWeek.Monday, 9, 0),
        trTimeZone // YENİ KULLANIM
    );
}

app.Run();