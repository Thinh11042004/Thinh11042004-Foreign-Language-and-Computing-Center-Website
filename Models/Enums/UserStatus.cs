namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums
{
    public enum UserStatus
    {
        Active = 1,
        Inactive = 2,
        Graduated = 3,
        Suspended = 4,
        Transferred = 5
    }

    public enum EnrollmentStatus
    {
        Studying = 1,
        Complete = 2,
        Dropped = 3,
        Suspended = 4,
        Transferred = 5,
    }

    public enum RegistrationStatus
    {
        Pending = 0,      // Chờ duyệt
        Approved = 1,     // Đã duyệt, chờ phân lớp
        Enrolled = 2,     // Đã xếp lớp
        Cancelled = 3,    // Bị hủy
        WaitingForClass = 4 // Chờ phân lớp
    }
}
