using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Student
{
    public class StudentDetailsViewModel
    {
        public Students Student { get; set; } = new();
        public List<Enrollment> ActiveEnrollments { get; set; } = new();
        public decimal TotalPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public double AttendanceRate { get; set; }
    }
}
