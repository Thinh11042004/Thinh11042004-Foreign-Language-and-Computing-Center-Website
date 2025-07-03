using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Report
{
    public class StudentProgressReportViewModel
    {
        public int? CourseId { get; set; }
        public List<StudentProgress> StudentProgresses { get; set; } = new();
        public double AverageProgress { get; set; }
        public int CompletionRate { get; set; }
        public int TotalStudents { get; set; }
    }
}
