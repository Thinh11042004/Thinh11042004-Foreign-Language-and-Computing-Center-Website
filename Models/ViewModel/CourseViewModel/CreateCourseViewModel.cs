using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.CourseViewModel
{
    public class CreateCourseViewModel
    {
        public int id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public CourseType Type { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [Required]
        public CourseLevel Level { get; set; }

        [Required]
        public long Fee { get; set; }

        [Required]
        [Range(1, 1000)]
        public int DurationHours { get; set; }

        [Required]
        [Range(1, 100)]
        public int MaxStudents { get; set; }

    }
}
