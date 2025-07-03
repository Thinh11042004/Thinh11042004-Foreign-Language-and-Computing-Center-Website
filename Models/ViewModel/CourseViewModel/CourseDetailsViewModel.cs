using CourseModel = Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses.Course;


namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Course
{
    public class CourseDetailsViewModel
    {
        public CourseModel Course { get; set; }
        public int TotalEnrollments { get; set; }
        public int ActiveClasses { get; set; }
        public double AverageRating { get; set; }
    }
}
