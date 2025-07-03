using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Filters
{
    public class StatusFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            var status = context.HttpContext.Request.Query["statusFilter"].ToString();
            if (controller != null)
            {
                controller.ViewBag.StatusFilter = status;
            }
        }
    }
}
