using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning
{
    public class StudentProgress
    {
        public int id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int? ModuleId { get; set; }
        public string SkillArea { get; set; } 
        public decimal CurrentScore { get; set; }
        public decimal MaxScore { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedAssignments { get; set; }
        public int TotalAssignments { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Notes { get; set; }

        public virtual Students Student { get; set; }
        public virtual Courses.Course Course { get; set; }
        public virtual CourseModule Module { get; set; }

    }
}
