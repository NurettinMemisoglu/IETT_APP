using IETT_APP.Application.Dtos;
using IETT_APP.WebMVC.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IETT_APP.WebMVC.Controllers
{
    // AllowAnonymous: Giriş yapmamış kullanıcıların bu controller'a erişebilmesi için şarttır.
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IAuthApiService _authApiService;

        public AuthController(IAuthApiService authApiService)
        {
            _authApiService = authApiService;
        }

        // ============================================================
        // LOGIN (GİRİŞ)
        // ============================================================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Eğer kullanıcı zaten giriş yapmışsa, tekrar Login sayfasını görmesin.
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Eğer bir sayfadan "yetkisiz" diye kovulup buraya geldiyse (ReturnUrl varsa),
                // onu tekrar o sayfaya göndermek döngü yaratır. Ana sayfaya atalım.
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                // Normal bir şekilde geldiyse rolüne uygun panele yönlendir.
                var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
                return RedirectByRole(roles);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var loginDto = new LoginUserDto
            {
                Email = model.Email,
                Password = model.Password
            };

            // API'ye İstek At
            var authResponse = await _authApiService.LoginAsync(loginDto);

            if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(authResponse.AccessToken);

                // 1. Temel Token'ları Listeye Ekle
                var claims = new List<Claim>
                {
                    new Claim("AccessToken", authResponse.AccessToken),
                    new Claim("RefreshToken", authResponse.RefreshToken)
                };

                // 2. API Claimlerini MVC Standartlarına Çevir (MAPPING)
                // Bu adım çok kritiktir. API "nameid" gönderir, MVC "NameIdentifier" arar.
                foreach (var claim in jwtToken.Claims)
                {
                    // ROL MAPPING
                    if (claim.Type == "role" || claim.Type == ClaimTypes.Role || claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    {
                        claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                    }
                    // ID MAPPING (nameid -> NameIdentifier)
                    else if (claim.Type == "nameid" || claim.Type == "sub" || claim.Type == "id")
                    {
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, claim.Value));
                    }
                    // İSİM MAPPING (unique_name -> Name)
                    else if (claim.Type == "unique_name" || claim.Type == "name")
                    {
                        claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                    }
                    // EMAIL MAPPING
                    else if (claim.Type == "email")
                    {
                        claims.Add(new Claim(ClaimTypes.Email, claim.Value));
                    }
                    else
                    {
                        // Diğerlerini olduğu gibi ekle (exp, iat, nbf hariç tutulabilir ama zararı yok)
                        claims.Add(claim);
                    }
                }

                // 3. Kimlik Kartını (Identity) Oluştur
                // Role ve Name tiplerini açıkça belirtiyoruz ki User.IsInRole() doğru çalışsın.
                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    ClaimTypes.Name,
                    ClaimTypes.Role);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // "Beni Hatırla" varsayılan açık
                    ExpiresUtc = jwtToken.ValidTo,
                    AllowRefresh = true
                };

                // 4. Tarayıcıya Cookie Yaz (Oturum Başlat)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // 5. Yönlendirme
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                // Rolleri yeni oluşturduğumuz claim listesinden çekip yönlendir
                var roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                return RedirectByRole(roles);
            }

            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(model);
        }

        // ============================================================
        // REGISTER (KAYIT)
        // ============================================================

        [HttpGet]
        public IActionResult Register(bool fromAdmin = false)
        {
            // Eğer admin panelinden gelmiyorsa ve zaten giriş yapmışsa, kayıt sayfasına girmesine gerek yok
            if (!fromAdmin && User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewBag.FromAdmin = fromAdmin;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model, bool fromAdmin = false)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FromAdmin = fromAdmin;
                return View(model);
            }

            var registerDto = new RegisterUserDto
            {
                Name = model.Name,
                Surname = model.Surname,
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword
            };

            var authResponse = await _authApiService.RegisterAsync(registerDto);

            if (authResponse != null)
            {
                if (fromAdmin)
                {
                    TempData["SuccessMessage"] = $"Kullanıcı '{model.Email}' başarıyla oluşturuldu.";
                    return RedirectToAction("Index", "User", new { area = "Admin" });
                }
                else
                {
                    TempData["SuccessMessage"] = "Kayıt başarılı! Lütfen giriş yapınız.";
                    return RedirectToAction("Login");
                }
            }

            ModelState.AddModelError("", "Kayıt başarısız. Lütfen bilgilerinizi kontrol ediniz.");
            ViewBag.FromAdmin = fromAdmin;
            return View(model);
        }

        // ============================================================
        // ŞİFRE DEĞİŞTİRME (AJAX Çağrısı İçin)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Model validasyon hatalarını topla
                var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Form hatalı.", errors = modelErrors });
            }

            var dto = new ChangePasswordDto
            {
                CurrentPassword = model.CurrentPassword,
                NewPassword = model.NewPassword
            };

            var result = await _authApiService.ChangePasswordAsync(dto);

            if (result.Succeeded)
            {
                return Ok(new { message = "Şifre başarıyla değiştirildi." });
            }

            // BURADA DÜZELTME: result.Message boş olsa bile Errors listesini gönderiyoruz
            // JS tarafında bu 'errors' dizisini göstereceğiz.
            var failMessage = !string.IsNullOrEmpty(result.Message) ? result.Message : "İşlem başarısız.";

            return BadRequest(new
            {
                message = failMessage,
                errors = result.Errors // Hata detaylarını gönder
            });
        }

        // ============================================================
        // LOGOUT & ERİŞİM REDDİ
        // ============================================================

        [HttpGet]
        public IActionResult Logout()
        {
            // Zaten çıkış yapmışsa Login'e at
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }
            return View(); // Logout onay sayfasını göster
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                await _authApiService.LogoutAsync();
            }

            // Cookie'yi sil
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Çıkış sonrası direkt Login sayfasına yönlendir
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View(); // 403 Hata Sayfası
        }

        // ============================================================
        // YARDIMCI METOTLAR
        // ============================================================

        private IActionResult RedirectByRole(IEnumerable<string> roles)
        {
            if (roles.Contains("Admin")) return RedirectToAction("Index", "Home", new { area = "Admin" });

            if (roles.Contains("Chief")) return RedirectToAction("Index", "Home", new { area = "Chief" });

            if (roles.Contains("Planner")) return RedirectToAction("Index", "Home", new { area = "Planner" });

            if (roles.Contains("Driver")) return RedirectToAction("Index", "Home", new { area = "Driver" }); // Driver Dashboard

            // Varsayılan (Rolsüz veya Sadece User)
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}