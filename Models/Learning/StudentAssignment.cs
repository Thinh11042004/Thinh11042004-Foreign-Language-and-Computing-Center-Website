using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning
{
    public class StudentAssignment
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string SubmissionContent { get; set; }
        public string AttachedFiles { get; set; }
        public DateTime SubmittedAt { get; set; }
        public decimal? Score { get; set; }
        public string Feedback { get; set; }
        public string GradedBy { get; set; }
        public DateTime? GradedAt { get; set; }
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;

        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public double Percentage { get; set; }

        public virtual Enrollment Enrollment { get; set; }
        public virtual Assignment Assignment { get; set; }
        public virtual Students Student { get; set; }
    }
}
