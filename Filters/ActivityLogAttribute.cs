using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Filters
{

    //Bộ lọc ghi lại hoạt động
    public class ActivityLogAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var dbContext = context.HttpContext.RequestServices.GetService<ApplicationDbContext>();
                var userManager = context.HttpContext.RequestServices.GetService<UserManager<ApplicationUser>>();

                var user = userManager.GetUserAsync(context.HttpContext.User).Result; // Sử dụng .Result vì phương thức này là đồng bộ
                var action = $"{context.ActionDescriptor.RouteValues["controller"]}.{context.ActionDescriptor.RouteValues["action"]}";

                var auditLog = new AuditLog
                {
                    UserId = user?.Id,
                    Action = action,
                    EntityType = context.ActionDescriptor.RouteValues["controller"],
                    Timestamp = DateTime.UtcNow,
                    IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.HttpContext.Request.Headers["User-Agent"].ToString()
                };

                dbContext.AuditLogs.Add(auditLog);
                dbContext.SaveChanges();
            }
        }
    }
}
