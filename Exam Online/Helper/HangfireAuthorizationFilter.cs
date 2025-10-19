using Hangfire.Dashboard;

namespace Exam_Online_API.Helper
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                return true;
            }

            return httpContext.User.Identity?.IsAuthenticated == true &&
                   (httpContext.User.IsInRole("Admin") || httpContext.User.IsInRole("SuperAdmin"));
        }
    }
}
