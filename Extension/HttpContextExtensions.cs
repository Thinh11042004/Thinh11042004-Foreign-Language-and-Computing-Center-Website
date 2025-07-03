using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Extension
{
    public static class HttpContextExtensions
    {
        public static async Task<ApplicationUser> GetCurrentUserAsync(this HttpContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                return await userManager.GetUserAsync(context.User);
            }
            return null;
        }

        public static string GetUserIpAddress(this HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        public static string GetUserAgent(this HttpContext context)
        {
            return context.Request.Headers["User-Agent"].ToString();
        }
    }
}
