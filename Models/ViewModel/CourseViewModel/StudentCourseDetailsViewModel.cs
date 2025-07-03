using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Course
{
    public class StudentCourseDetailsViewModel
    {
        public Enrollment Enrollment { get; set; } = new();
        public List<StudentProgress> Progress { get; set; } = new();
        public List<StudentAssignment> Assignments { get; set; } = new();
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new();

        // Thêm nếu cần thêm thông tin khác
        public Students Student { get; set; } = new();
    }
}
