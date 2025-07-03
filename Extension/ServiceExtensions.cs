using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Middlewares;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Setting;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Background;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Email;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Notification;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Traffic;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Extension
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Email Configuration
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // Custom Services
            services.AddScoped<ITrafficService, TrafficService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<INotificationService, NotificationService>();
            //services.AddScoped<IFinancialService, FinancialService>();
            //services.AddScoped<IReportService, ReportService>();

            // Background Services
            services.AddHostedService<AttendanceReminderService>();
            services.AddHostedService<DatabaseCleanupService>();

            return services;
        }

        public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<TrafficTrackingMiddleware>();
            app.UseMiddleware<UserActivityMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            return app;
        }
    }

}
