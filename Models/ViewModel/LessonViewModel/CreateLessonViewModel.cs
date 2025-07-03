using System;
using System.ComponentModel.DataAnnotations;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.LessonViewModel
{
    public class CreateLessonViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề buổi học")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày học")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
} 