using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;
using Microsoft.AspNetCore.Identity;
using NuGet.DependencyResolver;
using System.ComponentModel.DataAnnotations;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity
{
    public class ApplicationUser: IdentityUser 
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string FullName { get; set; }


        public string? Avatar { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày sinh.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public string? Address { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? LastLoginAt { get; set; }

        public bool MustChangePassword { get; set; } = false;

        // ---------- Navigation Properties ----------

        public UserStatus Status { get; set; } = UserStatus.Active;


        public virtual Teachers Teacher { get; set; }

        public virtual Students Student { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }

        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }
  
    }
}
