using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Middleware
{
    public class ForcePasswordChangeMiddleware
    {
        private readonly RequestDelegate _next;

        public ForcePasswordChangeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            var path = context.Request.Path.Value?.ToLower();

            var ignoredPaths = new[]
            {
                "/identity/account/login",
                "/identity/account/logout",
                "/identity/account/forgotpassword",
                "/identity/account/register"
            };

            if (ignoredPaths.Any(p => path.StartsWith(p)))
            {
                await _next(context);
                return;
            }

            if (context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);

                if (user != null && user.MustChangePassword)
                {
                    if (!path.StartsWith("/identity/account/manage/changepassword"))
                    {
                        context.Response.Redirect("/Identity/Account/Manage/ChangePassword");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    // ✅ Đây là phương thức mở rộng bắt buộc phải có
    public static class ForcePasswordChangeMiddlewareExtensions
    {
        public static IApplicationBuilder UseForcePasswordChange(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ForcePasswordChangeMiddleware>();
        }
    }
}
