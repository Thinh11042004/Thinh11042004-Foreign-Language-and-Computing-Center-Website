using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Report
{
    public class EnrollmentReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalEnrollments { get; set; }
        public Dictionary<string, int> EnrollmentsByCourse { get; set; } = new();
        public Dictionary<EnrollmentStatus, int> EnrollmentsByStatus { get; set; } = new();
        public List<MonthlyEnrollmentViewModel> MonthlyEnrollments { get; set; } = new();
    }
}
