using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class UserSessionApiService : IUserSessionApiService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiUserService _apiUserService;
        private readonly ILogger<UserSessionApiService> _logger; // Logger Eklendi

        public UserSessionApiService(
            IHttpContextAccessor httpContextAccessor,
            IApiUserService apiUserService,
            ILogger<UserSessionApiService> logger) // Constructor'a Logger Eklendi
        {
            _httpContextAccessor = httpContextAccessor;
            _apiUserService = apiUserService;
            _logger = logger;
        }

        public async Task RefreshSessionIfSelfAsync(string targetUserId)
        {
            _logger.LogInformation($"[RefreshSession] İşlem başladı. Hedef Kullanıcı ID: {targetUserId}");

            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("[RefreshSession] Kullanıcı oturumu bulunamadı.");
                return;
            }

            // --- DÜZELTME BURADA ---
            // ID'yi bulmak için olası tüm claim isimlerini kontrol et
            var currentUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? context.User.FindFirst("sub")?.Value
                             ?? context.User.FindFirst("nameid")?.Value
                             ?? context.User.FindFirst("id")?.Value;

            // Hata ayıklamak için cookie'deki tüm claim'leri loglayalım (Geçici)
            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogError("[RefreshSession] ID BULUNAMADI! Mevcut Claimler:");
                foreach (var claim in context.User.Claims)
                {
                    _logger.LogError($" - {claim.Type}: {claim.Value}");
                }
                return;
            }
            // -----------------------

            _logger.LogInformation($"[RefreshSession] Mevcut Kullanıcı ID: {currentUserId}");

            // ID kontrolü
            if (string.Equals(currentUserId, targetUserId, StringComparison.OrdinalIgnoreCase))
            {
                // ... Geri kalan kodlar AYNI ...
                _logger.LogInformation("[RefreshSession] ID Eşleşti. API'den güncel veri çekiliyor...");

                var updatedUserDto = await _apiUserService.GetByIdAsync(targetUserId);

                if (updatedUserDto != null)
                {
                    // ... (Claims oluşturma ve SignIn işlemleri) ...

                    // NOT: Claim listesini oluştururken ID'yi de garantiye alalım:
                    var newClaims = new List<Claim>();

                    // Tokenları koru...
                    var accessToken = context.User.FindFirst("AccessToken")?.Value;
                    var refreshToken = context.User.FindFirst("RefreshToken")?.Value;
                    if (!string.IsNullOrEmpty(accessToken)) newClaims.Add(new Claim("AccessToken", accessToken));
                    if (!string.IsNullOrEmpty(refreshToken)) newClaims.Add(new Claim("RefreshToken", refreshToken));

                    // ID'yi standart isimle tekrar ekle
                    newClaims.Add(new Claim(ClaimTypes.NameIdentifier, updatedUserDto.Id));
                    newClaims.Add(new Claim(ClaimTypes.Name, updatedUserDto.Email));
                    newClaims.Add(new Claim(ClaimTypes.Email, updatedUserDto.Email));

                    // Rolleri ekle...
                    if (updatedUserDto.RoleNames != null)
                    {
                        foreach (var roleName in updatedUserDto.RoleNames)
                        {
                            newClaims.Add(new Claim(ClaimTypes.Role, roleName));
                        }
                    }

                    // ... SignIn işlemleri AYNI ...
                    var claimsIdentity = new ClaimsIdentity(
                       newClaims,
                       CookieAuthenticationDefaults.AuthenticationScheme,
                       ClaimTypes.Name,
                       ClaimTypes.Role
                   );

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddHours(1),
                        IssuedUtc = DateTime.UtcNow,
                        AllowRefresh = true
                    };

                    await context.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    _logger.LogInformation("[RefreshSession] Cookie başarıyla yenilendi.");
                }
            }
            else
            {
                _logger.LogInformation("[RefreshSession] ID Eşleşmedi. Kendi rolünüzü değiştirmiyorsunuz.");
            }
        }

        public async Task UpdateProfileImageClaimAsync(string newRelativePath)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity == null || !context.User.Identity.IsAuthenticated) return;

            // 1. Mevcut Oturum Biletini Al
            // Bu, oturumun Properties (IsPersistent, ExpiresUtc) bilgilerini getirir.
            var authResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var identity = authResult.Principal?.Identity as ClaimsIdentity;

            if (identity == null) return;

            // 2. Eski Claim'i Bul ve Sil
            var existingClaim = identity.FindFirst("ProfileImage");
            if (existingClaim != null)
            {
                identity.RemoveClaim(existingClaim);
            }

            // 3. Yeni Claim'i Ekle (Yeni Yolu Sakla)
            identity.AddClaim(new Claim("ProfileImage", newRelativePath));

            // 4. Yeniden Oturum Açtır (Cookie'yi overwrite et)
            // authResult.Properties'i kullanıyoruz ki IsPersistent, ExpiresUtc ayarları korunsun.
            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authResult.Properties // <-- BURASI KRİTİK: Eski özellikler korunuyor
            );
        }
    }
}