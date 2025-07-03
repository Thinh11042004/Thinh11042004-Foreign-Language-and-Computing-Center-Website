using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses
{
    public class Course
    {

        public int id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public CourseType Type { get; set; } 
        public string Category { get; set; } 
        public CourseLevel Level { get; set; }
        public long Fee { get; set; }
        public int DurationHours { get; set; }
        public int MaxStudents { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<Classroom> Classes { get; set; }
        public virtual ICollection<CourseModule> Modules { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
