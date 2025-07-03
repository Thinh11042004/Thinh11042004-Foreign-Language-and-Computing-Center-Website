using CourseModel = Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses.Course;


namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Home
{
    public class HomeViewModel
    {
        public List<CourseModel> FeaturedCourses { get; set; } = new();
        public SiteStatistics Statistics { get; set; } = new();
    }
}
