using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Filters
{
    public class UserFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            string searchTerm = request.Query["searchTerm"];
            string roleFilter = request.Query["roleFilter"];
            string statusFilter = request.Query["statusFilter"];

            // Lưu vào ViewBag để controller có thể dùng lại
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                controller.ViewBag.SearchTerm = searchTerm;
                controller.ViewBag.RoleFilter = roleFilter;
                controller.ViewBag.StatusFilter = statusFilter;
            }

            base.OnActionExecuting(context);
        }
    }
}
