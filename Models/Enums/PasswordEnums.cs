using System.ComponentModel.DataAnnotations;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums
{
    public enum PasswordType
    {
        [Display(Name = "Sử dụng email làm mật khẩu")]
        UseEmail = 0,

        [Display(Name = "Mật khẩu tùy chỉnh")]
        Custom = 1
    }
}
