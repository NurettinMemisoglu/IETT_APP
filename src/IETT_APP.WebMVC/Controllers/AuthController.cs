using IETT_APP.Application.Dtos;
using IETT_APP.Application.Dtos.Driver;
using IETT_APP.WebMVC.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace IETT_APP.WebMVC.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IAuthApiService _authApiService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthApiService authApiService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _authApiService = authApiService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(returnUrl)) return RedirectToAction("Index", "Home", new { area = "" });
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

            var loginDto = new LoginUserDto { Email = model.Email, Password = model.Password };

            // 1. API'ye Login İsteği
            var authResponse = await _authApiService.LoginAsync(loginDto);

            if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(authResponse.AccessToken);

                var claims = new List<Claim>
                {
                    new Claim("AccessToken", authResponse.AccessToken),
                    new Claim("RefreshToken", authResponse.RefreshToken)
                };

                foreach (var claim in jwtToken.Claims)
                {
                    if (claim.Type == "role" || claim.Type == ClaimTypes.Role || claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                        claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                    else if (claim.Type == "nameid" || claim.Type == "sub" || claim.Type == "id")
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, claim.Value));
                    else if (claim.Type == "unique_name" || claim.Type == "name")
                        claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                    else if (claim.Type == "email")
                        claims.Add(new Claim(ClaimTypes.Email, claim.Value));
                    else
                        claims.Add(claim);
                }

                // Sürücü Resmi Çekme Mantığı
                if (claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Driver"))
                {
                    try
                    {
                        var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                        if (!string.IsNullOrEmpty(userId))
                        {
                            var client = _httpClientFactory.CreateClient();
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
                            var baseUrl = _configuration["ApiSettings:BaseUrl"]?.TrimEnd('/');
                            var requestUrl = $"{baseUrl}/api/drivers/user/{userId}";

                            var response = await client.GetAsync(requestUrl);
                            if (response.IsSuccessStatusCode)
                            {
                                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                                var driverDto = await response.Content.ReadFromJsonAsync<DriverDto>(jsonOptions);

                                if (driverDto != null && !string.IsNullOrEmpty(driverDto.ProfileImagePath))
                                {
                                    claims.Add(new Claim("ProfileImage", driverDto.ProfileImagePath));
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Resim çekilemezse login engellenmemeli.
                    }
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = jwtToken.ValidTo,
                    AllowRefresh = true
                };

                // Cookie oluşturuluyor
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // --- EKLENEN KISIM BURASI ---
                TempData["SuccessMessage"] = "Başarıyla giriş yapıldı.";
                // ----------------------------

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

                var roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                return RedirectByRole(roles);
            }

            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(model);
        }

        // ============================================================
        // REGISTER (KAYIT) - GÜNCELLENDİ
        // ============================================================

        [HttpGet]
        public IActionResult Register(bool fromAdmin = false)
        {
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
                PhoneNumber = model.PhoneNumber, // <--- Telefon Numarası Eklendi
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword
            };

            // HATA YÖNETİMİ İÇİN GÜNCELLENMİŞ KISIM
            // Not: _authApiService.RegisterAsync artık ServiceResult döndürmeli
            var result = await _authApiService.RegisterAsync(registerDto);

            if (result.Succeeded)
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

            // API'den gelen hataları (Örn: Şifre uyuşmuyor, Email var vb.) ekrana basıyoruz
            if (result.Errors != null)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("", err);
                }
            }
            else
            {
                ModelState.AddModelError("", "Kayıt işlemi başarısız.");
            }

            ViewBag.FromAdmin = fromAdmin;
            return View(model);
        }

        // ============================================================
        // ŞİFRE DEĞİŞTİRME
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
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

            var failMessage = !string.IsNullOrEmpty(result.Message) ? result.Message : "İşlem başarısız.";

            return BadRequest(new
            {
                message = failMessage,
                errors = result.Errors
            });
        }

        // ============================================================
        // LOGOUT
        // ============================================================

        [HttpGet]
        public IActionResult Logout()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                await _authApiService.LogoutAsync();
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectByRole(IEnumerable<string> roles)
        {
            if (roles.Contains("Admin")) return RedirectToAction("Index", "Home", new { area = "Admin" });
            if (roles.Contains("Chief")) return RedirectToAction("Index", "Home", new { area = "Chief" });
            if (roles.Contains("Planner")) return RedirectToAction("Index", "Home", new { area = "Planner" });
            if (roles.Contains("Driver")) return RedirectToAction("Index", "Home", new { area = "Driver" });
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}