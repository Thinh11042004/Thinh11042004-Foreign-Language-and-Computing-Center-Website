using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Class;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Dashboard
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int ActiveClasses { get; set; }
        public int PendingPayments { get; set; }
        public List<Enrollment> RecentEnrollments { get; set; } = new();
        public decimal MonthlyRevenue { get; set; }
        public List<ClassScheduleViewModel> ClassSchedule { get; set; } = new();
    }
}
