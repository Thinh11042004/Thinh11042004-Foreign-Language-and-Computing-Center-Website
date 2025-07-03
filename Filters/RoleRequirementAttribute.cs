using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Filters
{

    //Bộ lọc yêu cầu quyền hạn
    public class RoleRequirementAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RoleRequirementAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_roles.Any() && !_roles.Any(role => user.IsInRole(role)))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }

}
