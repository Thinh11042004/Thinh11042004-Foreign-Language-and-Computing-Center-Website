using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Feedback
{
    public class TeacherRating
    {
        public int id { get; set; }
        public int TeacherId { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public int Rating { get; set; } 
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAnonymous { get; set; }

        public virtual Teachers Teacher { get; set; }
        public virtual Students Student { get; set; }
        public virtual Classroom Class { get; set; }
    }
}
