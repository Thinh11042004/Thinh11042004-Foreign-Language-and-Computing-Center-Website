using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.System
{
    public class WebsiteVisit
    {
        public int id { get; set; }
        public string SessionId { get; set; }
        public string? UserId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Page { get; set; }
        public string Referrer { get; set; }
        public DateTime VisitTime { get; set; }
        public int Duration { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string Device { get; set; }
        public string Browser { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
