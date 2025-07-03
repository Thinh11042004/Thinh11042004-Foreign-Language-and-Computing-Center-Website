namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Teacher
{
    public class TeacherPerformanceStats
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalStudents { get; set; }
        public int ActiveClasses { get; set; }
        public int TotalHoursThisMonth { get; set; }
    }
}
