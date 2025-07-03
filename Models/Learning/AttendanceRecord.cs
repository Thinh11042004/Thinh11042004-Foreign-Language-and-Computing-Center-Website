using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning
{
    public class AttendanceRecord
    {

        public int id { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public int? LessonId { get; set; }
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public string MarkedBy { get; set; }
        public DateTime MarkedAt { get; set; }

        public virtual Students Student { get; set; }
        public virtual Classroom Class { get; set; }
        public virtual Lesson Lesson { get; set; }
    }
}
