using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Financial;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Report;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Report
{
    public interface IReportService
    {
        Task<RevenueReportViewModel> GenerateRevenueReportAsync(DateTime startDate, DateTime endDate);
        Task<EnrollmentReportViewModel> GenerateEnrollmentReportAsync(DateTime startDate, DateTime endDate);
        Task<TeacherPerformanceReportViewModel> GenerateTeacherPerformanceReportAsync();
        Task<StudentProgressReportViewModel> GenerateStudentProgressReportAsync(int? courseId);
        Task<decimal> GetMonthlyRevenueAsync();
        Task<decimal> GetTotalRevenueLast30DaysAsync();
        Task<int> GetTotalEnrollmentsLast30DaysAsync();
    }
}
