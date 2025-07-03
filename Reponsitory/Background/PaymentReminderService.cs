//using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
//using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Email;
//using Microsoft.EntityFrameworkCore;

//namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Background
//{
//    public class PaymentReminderService : BackgroundService
//    {
//        private readonly IServiceProvider _serviceProvider;
//        private readonly ILogger<PaymentReminderService> _logger;

//        public PaymentReminderService(IServiceProvider serviceProvider, ILogger<PaymentReminderService> logger)
//        {
//            _serviceProvider = serviceProvider;
//            _logger = logger;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    using (var scope = _serviceProvider.CreateScope())
//                    {
//                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

//                        var overduePayments = await context.Payments
//                            .Include(p => p.Student.User)
//                            .Where(p => p.Status == PaymentStatus.Pending && p.DueDate < DateTime.Now)
//                            .ToListAsync(stoppingToken);

//                        foreach (var payment in overduePayments)
//                        {
//                            await emailService.SendNotificationAsync(
//                                payment.Student.User.Email,
//                                "Payment Overdue Reminder",
//                                $"Your payment of ${payment.Amount} is overdue. Please make payment as soon as possible.");

//                            payment.Status = PaymentStatus.Overdue;
//                        }

//                        await context.SaveChangesAsync(stoppingToken);
//                    }

//                    // Run every 24 hours
//                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Error occurred in PaymentReminderService");
//                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
//                }
//            }
//        }
//    }
//}
