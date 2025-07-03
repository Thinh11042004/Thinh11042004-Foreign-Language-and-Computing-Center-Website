using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Finance;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Report;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Financial
{
    public class FinancialService : IFinancialService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FinancialService> _logger;

        public FinancialService(ApplicationDbContext context, ILogger<FinancialService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<decimal> GetMonthlyRevenueAsync()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid &&
                           p.PaidDate.Value.Month == currentMonth &&
                           p.PaidDate.Value.Year == currentYear)
                .SumAsync(p => p.Amount);
        }

        public async Task<List<MonthlyRevenueViewModel>> GetRevenueChartDataAsync()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid && p.PaidDate >= sixMonthsAgo)
                .GroupBy(p => new { p.PaidDate.Value.Year, p.PaidDate.Value.Month })
                .Select(g => new MonthlyRevenueViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(p => p.Amount)
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => r.Month)
                .ToListAsync();
        }

        public async Task CalculateMonthlySalariesAsync(int month, int year)
        {
            var teachers = await _context.Teachers.ToListAsync();

            foreach (var teacher in teachers)
            {
                var existingSalary = await _context.Salaries
                    .FirstOrDefaultAsync(s => s.TeacherId == teacher.id && s.Month == month && s.Year == year);

                if (existingSalary != null) continue;

                var teachingHours = await GetTeacherMonthlyHoursAsync(teacher.id, month, year);
                var teachingPayment = teachingHours * teacher.HourlyRate;
                var totalSalary = teacher.BaseSalary + teachingPayment;

                var salary = new Salary
                {
                    TeacherId = teacher.id,
                    Month = month,
                    Year = year,
                    BaseSalary = teacher.BaseSalary,
                    TeachingHours = teachingHours,
                    HourlyRate = teacher.HourlyRate,
                    TeachingPayment = teachingPayment,
                    Bonus = 0, // Calculate bonus logic here
                    Deductions = 0, // Calculate deductions here
                    TotalSalary = totalSalary,
                    Status = SalaryStatus.Calculated,
                    Notes = "Lương được tính tự động bởi hệ thống.",
                    ProcessedBy = "System"
                };

                _context.Salaries.Add(salary);
            }

            await _context.SaveChangesAsync();
        }

        public async Task ProcessPaymentAsync(int paymentId, PaymentMethod method, string transactionId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null) return;

            payment.Status = PaymentStatus.Paid;
            payment.Method = method;
            payment.TransactionId = transactionId;
            payment.PaidDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private async Task<int> GetTeacherMonthlyHoursAsync(int teacherId, int month, int year)
        {
            var lessons = await _context.Lessons
                .Where(l => l.Class.ClassTeachers.Any(ct => ct.TeacherId == teacherId && ct.IsPrimary) &&
                           l.ScheduledDate.Month == month &&
                           l.ScheduledDate.Year == year &&
                           l.Status == LessonStatus.Completed)
                .ToListAsync();

            return (int)lessons.Sum(l => l.Duration.TotalHours);
        }
    }
}
