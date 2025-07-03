using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses
{
    public class ClassTeacher
    {
        public int id { get; set; }
        public int ClassId { get; set; }
        public int TeacherId { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime AssignedDate { get; set; }
        //public string Role { get; set; }

        public virtual Classroom Class { get; set; }
        public virtual Teachers Teacher { get; set; }
    }
}
