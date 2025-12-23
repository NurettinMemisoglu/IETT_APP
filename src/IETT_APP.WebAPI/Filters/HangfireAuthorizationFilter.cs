using Hangfire.Dashboard;

namespace IETT_APP.WebAPI.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            if (httpContext.User.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            if (httpContext.User.IsInRole("Admin") || httpContext.User.IsInRole("Chief") || httpContext.User.IsInRole("Planner"))
            {
                return true;
            }

            return false;
        }
    }
}