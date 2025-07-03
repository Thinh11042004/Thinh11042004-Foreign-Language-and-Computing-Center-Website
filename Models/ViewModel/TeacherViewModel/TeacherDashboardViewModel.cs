using System.Collections.Generic;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Feedback;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.TeacherViewModel
{
    public class TeacherMainDashboardViewModel
    {
        public Teachers Teacher { get; set; } = new();
        public List<Classroom> TodayClasses { get; set; } = new();
        public List<Classroom> ActiveClasses { get; set; } = new();
        public List<StudentAssignment> PendingAssignments { get; set; } = new();
        public List<TeacherRating> RecentRatings { get; set; } = new();
    }
} 