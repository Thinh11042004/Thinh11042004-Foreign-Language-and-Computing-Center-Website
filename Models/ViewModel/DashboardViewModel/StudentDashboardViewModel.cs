using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Shared;


namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Dashboard
{
    public class StudentDashboardViewModel
    {
        public Students Student { get; set; } = new();
        public List<Enrollment> ActiveEnrollments { get; set; } = new();
        public List<ClassroomItem> TodayClasses { get; set; } = new();
        public List<Assignment> PendingAssignments { get; set; } = new();
        public List<StudentAssignment> RecentGrades { get; set; } = new();
        public double OverallProgress { get; set; }
    }
}
