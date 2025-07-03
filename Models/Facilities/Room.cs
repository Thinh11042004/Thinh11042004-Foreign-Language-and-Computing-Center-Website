using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Facilities
{
    public class Room
    {

        public int id { get; set; }

        [Required]
        [DisplayName("Tên phòng")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Loại phòng")]
        public RoomType Type { get; set; }

        [Required]
        [DisplayName("Sức chứa")]
        public int Capacity { get; set; }

        [Required]
        [DisplayName("Vị trí")]
        public string? Location { get; set; }

        [Required]
        [DisplayName("Đang hoạt động")]
        public bool IsActive { get; set; } = true;
        

        [DisplayName("Mô tả")]
        public string? Description { get; set; }

     
        public virtual ICollection<Classroom>? Classes { get; set; }
        public virtual ICollection<RoomBooking>? Bookings { get; set; }
    }
}
