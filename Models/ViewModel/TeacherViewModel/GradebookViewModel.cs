using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.TeacherViewModel
{
    public class GradebookViewModel
    {
        public List<Classroom> Classes { get; set; } = new List<Classroom>();
        public List<StudentGradeViewModel> StudentGrades { get; set; } = new List<StudentGradeViewModel>();
    }

    public class StudentGradeViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public List<AssignmentGradeViewModel> Assignments { get; set; } = new List<AssignmentGradeViewModel>();
    }

    public class AssignmentGradeViewModel
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public int LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public int? Score { get; set; }
        public AssignmentStatus Status { get; set; }
        public string SubmissionContent { get; set; } = string.Empty;
        public string AttachedFiles { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public int? StudentAssignmentId { get; set; }
    }
} 