using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Traffic
{
    public class TrafficService : ITrafficService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TrafficService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task LogVisitAsync(HttpContext context)
        {
            var sessionId = context.Session.Id;
            var userId = context.User.Identity.IsAuthenticated
                ? (await _userManager.GetUserAsync(context.User))?.Id
                : null;


            var visit = new WebsiteVisit
            {
                SessionId = sessionId,
                UserId = userId,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                Page = context.Request.Path,
                Referrer = context.Request.Headers["Referer"].ToString(),
                VisitTime = DateTime.UtcNow,
                Device = GetDeviceType(context.Request.Headers["User-Agent"].ToString()),
                Browser = GetBrowserType(context.Request.Headers["User-Agent"].ToString())
            };

            _context.WebsiteVisits.Add(visit);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WebsiteVisit>> GetVisitStatsAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.WebsiteVisits
                .Where(v => v.VisitTime >= startDate && v.VisitTime <= endDate)
                .ToListAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            var thirtyMinutesAgo = DateTime.UtcNow.AddMinutes(-30);
            return await _context.WebsiteVisits
                .Where(v => v.VisitTime >= thirtyMinutesAgo)
                .Select(v => v.SessionId)
                .Distinct()
                .CountAsync();
        }

        private string GetDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            userAgent = userAgent.ToLower();

            if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone"))
                return "Mobile";
            else if (userAgent.Contains("tablet") || userAgent.Contains("ipad"))
                return "Tablet";
            else
                return "Desktop";
        }

        private string GetBrowserType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            userAgent = userAgent.ToLower();

            if (userAgent.Contains("chrome")) return "Chrome";
            else if (userAgent.Contains("firefox")) return "Firefox";
            else if (userAgent.Contains("safari")) return "Safari";
            else if (userAgent.Contains("edge")) return "Edge";
            else return "Other";
        }
    }
}
