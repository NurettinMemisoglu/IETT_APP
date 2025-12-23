using IETT_APP.Application.Interfaces; // Servisinizin olduğu namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace IETT_APP.Web.Filters
{
    public class CheckProfileCompletionFilter : IAsyncActionFilter
    {
        private readonly IDriverService _driverService; // Profil kontrolü yapacak servis

        public CheckProfileCompletionFilter(IDriverService driverService)
        {
            _driverService = driverService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Gidilen Action'ın adını al
            var actionName = context.ActionDescriptor.RouteValues["action"]?.ToString();
            var controllerName = context.ActionDescriptor.RouteValues["controller"]?.ToString();

            // EĞER KULLANICI ZATEN "CompleteProfile" SAYFASINDAYSA KONTROL ETME, GEÇ
            if (controllerName == "Driver" && actionName == "CompleteProfile")
            {
                await next();
                return;
            }

            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                // Veritabanından veya Cache'den kullanıcının profili tamamlayıp tamamlamadığını sor
                var isProfileComplete = await _driverService.IsProfileCompleteAsync(userId);

                if (!isProfileComplete)
                {
                    // Eğer tamamlamamışsa, direkt "Profil Tamamla" sayfasına yönlendir
                    context.Result = new RedirectToActionResult("CompleteProfile", "Driver", null);
                    return;
                }
            }

            // Her şey yolundaysa sayfayı aç
            await next();
        }
    }
}