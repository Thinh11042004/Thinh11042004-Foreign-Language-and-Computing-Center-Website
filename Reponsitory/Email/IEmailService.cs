using System.Threading.Tasks;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath, bool isHtml = false);
        Task SendNotificationAsync(string email, string subject, string message);
    }
}
