using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Filters
{

    //Bộ lọc giới hạn tần suất yêu cầu
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;
        private static readonly Dictionary<string, List<DateTime>> _requests = new();

        public RateLimitAttribute(int maxRequests = 100, int timeWindowMinutes = 1)
        {
            _maxRequests = maxRequests;
            _timeWindow = TimeSpan.FromMinutes(timeWindowMinutes);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var key = GetClientKey(context.HttpContext);
            var now = DateTime.UtcNow;

            if (!_requests.ContainsKey(key))
            {
                _requests[key] = new List<DateTime>();
            }

            var requests = _requests[key];
            requests.RemoveAll(r => now - r > _timeWindow);

            if (requests.Count >= _maxRequests)
            {
                context.Result = new StatusCodeResult(429); 
                return;
            }

            requests.Add(now);
            base.OnActionExecuting(context);
        }

        private string GetClientKey(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var userId = context.User.Identity.IsAuthenticated ?
                        context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value :
                        "anonymous";
            return $"{ip}-{userId}";
        }
    }
}
