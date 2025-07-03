using System.ComponentModel.DataAnnotations;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums
{
    public enum CourseType
    {
        [Display(Name = "Ngôn ngữ")]
        Language = 1,
        [Display(Name = "Công nghệ thông tin")]
        IT = 2
    }

    public enum CourseLevel
    {
        [Display(Name = "Cơ bản")]
        Beginner = 1,
        [Display(Name = "Sơ cấp")]
        Elementary = 2,
        [Display(Name = "Trung cấp")]
        Intermediate = 3,
        [Display(Name = "Trung cấp cao")]
        UpperIntermediate = 4,
        [Display(Name = "Nâng cao")]
        Advanced = 5,
        [Display(Name = "Chuyên gia")]
        Expert = 6
    }
}
