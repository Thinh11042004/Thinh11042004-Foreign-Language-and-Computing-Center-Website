using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Class
{
    public class TeacherClassDetailsViewModel
    {
        public Classroom Class { get; set; }
        public List<Students> Students { get; set; }
        public List<Lesson> Lessons { get; set; }
        public List<Assignment> UpcomingAssignments { get; set; }
    }
}
