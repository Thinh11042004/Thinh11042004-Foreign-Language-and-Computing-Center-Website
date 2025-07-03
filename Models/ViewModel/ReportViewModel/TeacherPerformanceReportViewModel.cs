using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Teacher;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Report
{
    public class TeacherPerformanceReportViewModel
    {
        public List<TeacherPerformanceStats> Teachers { get; set; } = new();
        public double AverageRating { get; set; }
        public int TotalTeachers { get; set; }
        public int ActiveTeachers { get; set; }
    }
}
