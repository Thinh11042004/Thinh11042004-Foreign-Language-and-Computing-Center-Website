using CourseModel = Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses.Course;

using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.CourseViewModel
{
    public class CourseListViewModel
    {
        public List<CourseModel> Courses { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public CourseType? SelectedType { get; set; }
        public Dictionary<int, List<Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users.Students>> CourseStudents { get; set; } = new();
        public Dictionary<int, List<Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses.Classroom>> CourseClasses { get; set; } = new();
        public Dictionary<int, List<Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users.Teachers>> CourseTeachers { get; set; } = new();
    }
}
