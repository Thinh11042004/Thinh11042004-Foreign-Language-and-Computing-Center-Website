using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Facilities
{
    public class RoomBooking
    {

        public int id { get; set; }
        public int RoomId { get; set; }
        public string BookedBy { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Purpose { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
        public string Notes { get; set; }

        public virtual Room Room { get; set; }
    }
}
