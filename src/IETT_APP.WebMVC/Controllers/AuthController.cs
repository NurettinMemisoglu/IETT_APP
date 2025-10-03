using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCProject.Services.Interfaces;
using MVCProject.ViewModels;
using System.Security.Claims;

namespace IETT_APP.WebMVC.Controllers
{
    [Route("[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly IApiService _apiService;

        public AuthController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _apiService.RegisterAsync(model);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            // Kullanıcıyı kayıt sonrası otomatik login yap
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.UserName),
                new Claim(ClaimTypes.Role, "User")
            };
            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true, // tarayıcı kapansa bile cookie kalır
                    ExpiresUtc = DateTime.UtcNow.AddHours(1)
                });

            Console.WriteLine("Kayıt ve giriş başarılı!");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _apiService.LoginAsync(model);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            // Cookie için claim oluştur
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.UserName),
                new Claim(ClaimTypes.Role, "User")
            };
            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(1)
                });

            // Session'a token kaydet
            HttpContext.Session.SetString("JwtToken", result.Token);

            // ApiService içindeki HttpClient header set et
            _apiService.SetTokenHeader(result.Token);

            Console.WriteLine("Giriş başarılı!");
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            // Hem Cookie hem de Session temizle
            await HttpContext.SignOutAsync("MyCookieAuth"); // cookieyi sil
            await HttpContext.SignOutAsync();               // default varsa onu da sil

            HttpContext.Session.Clear();                    // tüm session temizle
            _apiService.RemoveTokenHeader();                // httpclient header reset

            // Kullanıcıyı ana sayfaya yönlendir
            return RedirectToAction("Index", "Home");
        }

    }
}
