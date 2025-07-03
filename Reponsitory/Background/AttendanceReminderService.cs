using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Notification;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Background
{
    public class AttendanceReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AttendanceReminderService> _logger;

        public AttendanceReminderService(IServiceProvider serviceProvider, ILogger<AttendanceReminderService> logger)
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
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var tomorrow = DateTime.Today.AddDays(1);
                        var tomorrowDayOfWeek = tomorrow.DayOfWeek;

                        var classesTomorrow = await context.Classes
                            .Include(c => c.Enrollments)
                                .ThenInclude(e => e.Student.User)
                            .Where(c => c.Status == ClassStatus.Active &&
                                       c.Schedule.Contains(tomorrowDayOfWeek.ToString()))
                            .ToListAsync(stoppingToken);

                        foreach (var classEntity in classesTomorrow)
                        {
                            foreach (var enrollment in classEntity.Enrollments.Where(e => e.Status == EnrollmentStatus.Studying))
                            {
                                await notificationService.SendNotificationAsync(
                                    enrollment.Student.UserId,
                                    "Class Reminder",
                                    $"You have {classEntity.Name} class tomorrow.",
                                    NotificationType.Class);
                            }
                        }
                    }

                    // Run daily at 6 PM
                    var now = DateTime.Now;
                    var nextRun = DateTime.Today.AddHours(18);
                    if (nextRun <= now)
                        nextRun = nextRun.AddDays(1);

                    await Task.Delay(nextRun - now, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in AttendanceReminderService");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }
    }
}
