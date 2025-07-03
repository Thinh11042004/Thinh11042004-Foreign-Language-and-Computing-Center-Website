namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Class
{
    public class ClassScheduleViewModel
    {
        public string ClassName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public string Schedule { get; set; } = string.Empty;
    }
}
