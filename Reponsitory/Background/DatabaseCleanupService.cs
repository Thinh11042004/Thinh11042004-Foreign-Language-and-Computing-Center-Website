using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Background
{
    public class DatabaseCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseCleanupService> _logger;

        public DatabaseCleanupService(IServiceProvider serviceProvider, ILogger<DatabaseCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        // Clean old audit logs (older than 1 year)
                        var oldAuditLogs = await context.AuditLogs
                            .Where(al => al.Timestamp < DateTime.UtcNow.AddYears(-1))
                            .ToListAsync(stoppingToken);

                        context.AuditLogs.RemoveRange(oldAuditLogs);

                        // Clean old website visits (older than 6 months)
                        var oldVisits = await context.WebsiteVisits
                            .Where(wv => wv.VisitTime < DateTime.UtcNow.AddMonths(-6))
                            .ToListAsync(stoppingToken);

                        context.WebsiteVisits.RemoveRange(oldVisits);

                        // Clean old notifications (older than 3 months and read)
                        var oldNotifications = await context.Notifications
                            .Where(n => n.IsRead && n.CreatedAt < DateTime.UtcNow.AddMonths(-3))
                            .ToListAsync(stoppingToken);

                        context.Notifications.RemoveRange(oldNotifications);

                        await context.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation($"Database cleanup completed. Removed {oldAuditLogs.Count} audit logs, {oldVisits.Count} visits, {oldNotifications.Count} notifications");
                    }

                    // Run cleanup daily at 2 AM
                    var now = DateTime.Now;
                    var nextRun = DateTime.Today.AddDays(1).AddHours(2);
                    var delay = nextRun - now;

                    await Task.Delay(delay, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in DatabaseCleanupService");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }
    }
}
