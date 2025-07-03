using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Facilities;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses
{
    public class Classroom
    {
        public int id { get; set; }
        public string Name { get; set; }

        public int CourseId { get; set; }
        public int RoomId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Schedule { get; set; }
        public int MaxStudents { get; set; }

        public ClassStatus Status { get; set; } = ClassStatus.Planned;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Course? Course { get; set; }
        public virtual Room? Room { get; set; }
        public virtual ICollection<ClassTeacher>? ClassTeachers { get; set; }
        public virtual ICollection<Enrollment>? Enrollments { get; set; }
        public virtual ICollection<Lesson>? Lessons { get; set; }
        public virtual ICollection<AttendanceRecord>? AttendanceRecords { get; set; }
    }
}