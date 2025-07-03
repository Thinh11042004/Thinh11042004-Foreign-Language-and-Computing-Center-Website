using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Notification
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message, NotificationType type);
        Task SendBulkNotificationAsync(List<string> userIds, string title, string message, NotificationType type);
        Task<List<NotificationType>> GetUserNotificationsAsync(string userId, int count = 10);
        Task SendEnrollmentNotificationAsync(Enrollment enrollment);

    }
}
