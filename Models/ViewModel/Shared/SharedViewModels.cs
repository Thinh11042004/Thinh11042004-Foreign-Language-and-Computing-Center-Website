namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Shared
{
    public class ClassroomItem 
    { 
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
    }

    public class LessonItem 
    { 
        public int Id { get; set; } 
        public string Title { get; set; } = string.Empty; 
    }
} 