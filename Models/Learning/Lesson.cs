using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning
{
    public class Lesson
    {
        public int id { get; set; }
        public int ClassId { get; set; }
        public int? ModuleId { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TimeSpan? Duration { get; set; } = null;
        public LessonStatus Status { get; set; } = LessonStatus.Scheduled;

        public virtual Classroom Class { get; set; }
        public virtual CourseModule Module { get; set; }
        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }

    }
}
