using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Traffic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnalyticsApiController : ControllerBase
    {
        private readonly ITrafficService _trafficService;
        private readonly ApplicationDbContext _context;

        public AnalyticsApiController(ITrafficService trafficService, ApplicationDbContext context)
        {
            _trafficService = trafficService;
            _context = context;
        }

        [HttpPost("TrackPageVisit")]
        public async Task<IActionResult> TrackPageVisit([FromBody] PageVisitRequest request)
        {
            await _trafficService.LogVisitAsync(HttpContext);
            return Ok();
        }

        [HttpGet("VisitStats")]
        public async Task<IActionResult> GetVisitStats(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddDays(-30);
            endDate ??= DateTime.Now;

            var visits = await _trafficService.GetVisitStatsAsync(startDate.Value, endDate.Value);

            var stats = new
            {
                totalVisits = visits.Count,
                uniqueVisitors = visits.GroupBy(v => v.IpAddress).Count(),
                averageSessionDuration = visits.Average(v => v.Duration),
                topPages = visits.GroupBy(v => v.Page)
                               .OrderByDescending(g => g.Count())
                               .Take(10)
                               .Select(g => new { page = g.Key, visits = g.Count() }),
                deviceBreakdown = visits.GroupBy(v => v.Device)
                                      .Select(g => new { device = g.Key, count = g.Count() }),
                browserBreakdown = visits.GroupBy(v => v.Browser)
                                       .Select(g => new { browser = g.Key, count = g.Count() })
            };

            return Ok(stats);
        }

        [HttpGet("ActiveUsers")]
        public async Task<IActionResult> GetActiveUsers()
        {
            var activeUsers = await _trafficService.GetActiveUsersCountAsync();
            return Ok(new { activeUsers });
        }
    }

    public class PageVisitRequest
    {
        public string Page { get; set; }
    }
}
