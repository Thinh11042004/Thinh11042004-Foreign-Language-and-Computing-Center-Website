using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses
{
    public class CourseModule
    {
        public int id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }
        public int EstimatedHours { get; set; }
        public string Content { get; set; } 
        public string Resources { get; set; } 

        public virtual Course Course { get; set; }
        public virtual ICollection<Lesson> Lessons { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }

    }
}
