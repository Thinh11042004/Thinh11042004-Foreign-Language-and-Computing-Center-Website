using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Financial;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Report;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Teacher;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Report
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RevenueReportViewModel> GenerateRevenueReportAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await _context.Payments
                .Where(p => p.PaidDate >= startDate && p.PaidDate <= endDate && p.Status == PaymentStatus.Paid)
                .ToListAsync();

            return new RevenueReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = payments.Sum(p => p.Amount),
                PaymentsByType = payments.GroupBy(p => p.Type)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
                PaymentsByMethod = payments.GroupBy(p => p.Method)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
                MonthlyBreakdown = payments.GroupBy(p => new { p.PaidDate.Value.Year, p.PaidDate.Value.Month })
                    .Select(g => new MonthlyRevenueViewModel
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(p => p.Amount)
                    })
                    .ToList()
            };
        }

        public async Task<EnrollmentReportViewModel> GenerateEnrollmentReportAsync(DateTime startDate, DateTime endDate)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.EnrollmentDate >= startDate && e.EnrollmentDate <= endDate)
                .ToListAsync();

            return new EnrollmentReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalEnrollments = enrollments.Count,
                EnrollmentsByCourse = enrollments.GroupBy(e => e.Course.Name)
                    .ToDictionary(g => g.Key, g => g.Count()),
                EnrollmentsByStatus = enrollments.GroupBy(e => e.Status)
                    .ToDictionary(g => g.Key, g => g.Count()),
                MonthlyEnrollments = enrollments.GroupBy(e => new { e.EnrollmentDate.Year, e.EnrollmentDate.Month })
                    .Select(g => new MonthlyEnrollmentViewModel
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .ToList()
            };
        }

        public async Task<TeacherPerformanceReportViewModel> GenerateTeacherPerformanceReportAsync()
        {
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Ratings)
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class.Enrollments)
                .ToListAsync();

            var teacherStats = teachers.Select(t => new TeacherPerformanceStats
            {
                TeacherId = t.id,
                TeacherName = t.User.FullName,
                AverageRating = t.Ratings.Any() ? t.Ratings.Average(r => r.Rating) : 0,
                TotalStudents = t.ClassTeachers.SelectMany(ct => ct.Class.Enrollments).Count(),
                ActiveClasses = t.ClassTeachers.Count(ct => ct.Class.Status == ClassStatus.Active),
                TotalHoursThisMonth = GetTeacherMonthlyHours(t.id)
            }).ToList();

            return new TeacherPerformanceReportViewModel
            {
                Teachers = teacherStats,
                AverageRating = teacherStats.Average(t => t.AverageRating),
                TotalTeachers = teachers.Count,
                ActiveTeachers = teachers.Count(t => t.IsAvailable)
            };
        }

        public async Task<StudentProgressReportViewModel> GenerateStudentProgressReportAsync(int? courseId)
        {
            var query = _context.StudentProgresses
                .Include(sp => sp.Student.User)
                .Include(sp => sp.Course)
                .AsQueryable();

            if (courseId.HasValue)
            {
                query = query.Where(sp => sp.CourseId == courseId.Value);
            }

            var progresses = await query.ToListAsync();

            return new StudentProgressReportViewModel
            {
                CourseId = courseId,
                StudentProgresses = progresses,
                AverageProgress = progresses.Any() ?
                    progresses.Average(p => (double)p.CompletedLessons / p.TotalLessons * 100) : 0,
                CompletionRate = progresses.Count(p => p.CompletedLessons == p.TotalLessons),
                TotalStudents = progresses.Select(p => p.StudentId).Distinct().Count()
            };
        }

        private int GetTeacherMonthlyHours(int teacherId)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            return _context.Lessons
                .Where(l => l.Class.ClassTeachers.Any(ct => ct.TeacherId == teacherId && ct.IsPrimary) &&
                           l.ScheduledDate.Month == currentMonth &&
                           l.ScheduledDate.Year == currentYear &&
                           l.Status == LessonStatus.Completed)
                .Sum(l => (int)l.Duration.TotalHours);
        }

        public async Task<decimal> GetMonthlyRevenueAsync()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1);

            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid &&
                            p.PaidDate >= startOfMonth &&
                            p.PaidDate <= endOfMonth)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
        }

        public async Task<decimal> GetTotalRevenueLast30DaysAsync()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid && p.PaidDate >= thirtyDaysAgo)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
        }

        public async Task<int> GetTotalEnrollmentsLast30DaysAsync()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            return await _context.Enrollments
                .Where(e => e.EnrollmentDate >= thirtyDaysAgo)
                .CountAsync();
        }
    }
}
