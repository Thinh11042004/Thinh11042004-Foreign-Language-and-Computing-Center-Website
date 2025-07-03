using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Traffic;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Middlewares
{
    public class TrafficTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public TrafficTrackingMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();

            // Only track page visits, not API calls or static files
            if (context.Request.Path.StartsWithSegments("/api") ||
                context.Request.Path.Value.Contains("."))
            {
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var trafficService = scope.ServiceProvider.GetRequiredService<ITrafficService>();

                // Set duration for the visit
                var sessionId = context.Session.Id;
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var recentVisit = await dbContext.WebsiteVisits
                    .Where(v => v.SessionId == sessionId && v.Page == context.Request.Path)
                    .OrderByDescending(v => v.VisitTime)
                    .FirstOrDefaultAsync();

                if (recentVisit != null &&
                    (DateTime.UtcNow - recentVisit.VisitTime).TotalMinutes < 30)
                {
                    recentVisit.Duration = (int)stopwatch.Elapsed.TotalSeconds;
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }

}
