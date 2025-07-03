using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning
{
    public class Assignment
    {
        public int id { get; set; }
        public int? LessonId { get; set; }
        public int? ModuleId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public AssignmentType Type { get; set; }
        public string Content { get; set; } 
        public int MaxScore { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsRequired { get; set; } = true;
        public string Instructions { get; set; }
        public string Resources { get; set; }

        public virtual Lesson Lesson { get; set; }
        public virtual CourseModule Module { get; set; }
        public virtual ICollection<StudentAssignment> StudentAssignments { get; set; }
    }
}
