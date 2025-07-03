using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.StudentViewModel
{
    public class StudentEnrollmentViewModel
    {
        public int StudentId { get; set; }
        public int EnrollmentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public UserStatus Status { get; set; }
        public string ClassName { get; set; }
        public Students Student { get; set; }
        public Enrollment Enrollment { get; set; }

        public RegistrationStatus RegistrationStatus { get; set; }

    }
}
