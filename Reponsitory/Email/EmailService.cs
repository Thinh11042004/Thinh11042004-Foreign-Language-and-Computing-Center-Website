using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Setting;
using Microsoft.Extensions.Options;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                message.To.Add(to);

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    UseDefaultCredentials = _emailSettings.UseDefaultCredentials,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
                };

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Log the error
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }

        public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath, bool isHtml = false)
        {
            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                message.To.Add(to);
                message.Attachments.Add(new Attachment(attachmentPath));

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    UseDefaultCredentials = _emailSettings.UseDefaultCredentials,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
                };

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Log the error
                throw new Exception($"Failed to send email with attachment: {ex.Message}", ex);
            }
        }

        public async Task SendNotificationAsync(string email, string subject, string message)
        {
            try
            {
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    UseDefaultCredentials = _emailSettings.UseDefaultCredentials,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
                };

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Log the error
                throw new Exception($"Failed to send notification email: {ex.Message}", ex);
            }
        }
    }
}
