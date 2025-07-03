using System.Security.Claims;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Middlewares
{
    public class UserActivityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserActivityMiddleware> _logger;

        public UserActivityMiddleware(RequestDelegate next, ILogger<UserActivityMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var action = $"{context.Request.Method} {context.Request.Path}";
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();

                _logger.LogInformation($"User {userId} performed {action} from {ipAddress}");
            }

            await _next(context);
        }
    }

}
