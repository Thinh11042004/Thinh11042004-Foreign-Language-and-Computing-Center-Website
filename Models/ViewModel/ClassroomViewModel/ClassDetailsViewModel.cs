using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;
using System.Collections.Generic;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.ClassroomViewModel
{
    public class ClassDetailsViewModel
    {
        public Classroom Class { get; set; } = new();
        public int TotalEnrollments { get; set; }
        public int TotalLessons { get; set; }
        public int TotalTeachers { get; set; }
    }
} 