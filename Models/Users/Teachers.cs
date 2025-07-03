using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Feedback;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users
{
    public class Teachers
    {
        public int id { get; set; }
        public string UserId { get; set; } 
        public string Specialization { get; set; } 
        public string SubjectAreas { get; set; } 
        public decimal HourlyRate { get; set; }
        public decimal BaseSalary { get; set; }
        public string Qualifications { get; set; } 
        public int ExperienceYears { get; set; }
        public string Bio { get; set; }
        public double Rating { get; set; }
        public int TotalRatings { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime JoinDate { get; set; }
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }


        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<ClassTeacher> ClassTeachers { get; set; }
        public virtual ICollection<TeacherSchedule> Schedules { get; set; }
        public virtual ICollection<TeacherRating> Ratings { get; set; }
    }
}
