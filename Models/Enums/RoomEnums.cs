using System.ComponentModel.DataAnnotations;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums
{
    public enum RoomType
    {
        [Display(Name = "Phòng học lý thuyết")]
        Classroom = 1,
        [Display(Name = "Phòng máy tính")]
        ComputerLab = 2,
        [Display(Name = "Phòng họp")]
        ConferenceRoom = 3,
        [Display(Name = "Thư viện")]
        Library = 4,
        [Display(Name = "Phòng nghe")]
        AudioLab = 5
    }

    public enum BookingStatus
    {
        Confirmed = 1,
        Cancelled = 2,
        InUse = 3,
        Completed = 4
    }
}
