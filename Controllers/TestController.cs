using System.Threading.Tasks;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Email;
using Microsoft.AspNetCore.Mvc;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    public class TestController : Controller
    {
        private readonly IEmailService _emailService;

        public TestController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<IActionResult> TestEmail()
        {
            try
            {
                await _emailService.SendEmailAsync(
                    "recipient@example.com", // Thay bằng email người nhận
                    "Test Email",
                    "This is a test email from the system.",
                    true
                );
                return Content("Email sent successfully!");
            }
            catch (System.Exception ex)
            {
                return Content($"Failed to send email: {ex.Message}");
            }
        }
    }
} 