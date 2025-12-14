using System.Net.Http.Headers;

namespace IETT_APP.WebMVC.Services.Infrastructure
{
    public class JwtDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Kullanıcının Cookie'sinden (veya Session'dan) Token'ı al
            // (Login olurken token'ı "AccessToken" isminde bir Claim veya Token olarak saklayacağız)
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null && httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated)
            {
                // Yöntem A: Eğer token'ı Claims içinde sakladıysak:
                var token = httpContext.User.FindFirst("AccessToken")?.Value;

                // Yöntem B: Eğer token'ı Authentication Properties içinde sakladıysak (Daha yaygın):
                // var token = await httpContext.GetTokenAsync("access_token");

                if (!string.IsNullOrEmpty(token))
                {
                    // 2. İsteğin Header'ına ekle
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}