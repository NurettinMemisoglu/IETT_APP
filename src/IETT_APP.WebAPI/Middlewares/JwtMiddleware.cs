namespace IETT_APP.WebAPI.Middlewares
{
    public static class JwtMiddleware
    {

        public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}