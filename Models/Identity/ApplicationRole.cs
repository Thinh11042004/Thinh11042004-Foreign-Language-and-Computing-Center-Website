using Microsoft.AspNetCore.Identity;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
