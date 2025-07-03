using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Teacher
{
    public class TeacherDetailsViewModel
    {
        public Teachers Teacher { get; set; }
        public List<Classroom> CurrentClasses { get; set; }
        public int TotalStudents { get; set; }
        public double AverageRating { get; set; }
        public int MonthlyTeachingHours { get; set; }
    }
}
