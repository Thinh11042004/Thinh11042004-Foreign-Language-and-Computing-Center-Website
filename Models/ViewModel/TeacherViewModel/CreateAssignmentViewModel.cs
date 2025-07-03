using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Shared;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.TeacherViewModel
{
    public class CreateAssignmentViewModel
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;
        [Required(ErrorMessage = "Điểm tối đa không được để trống")]
        [Range(1, 100, ErrorMessage = "Điểm phải từ 1 đến 100")]
        [Display(Name = "Điểm tối đa")]
        public int MaxScore { get; set; }
        [Required(ErrorMessage = "Hạn nộp không được để trống")]
        [Display(Name = "Hạn nộp")]
        public DateTime DueDate { get; set; }
        [Display(Name = "Hướng dẫn")]
        public string Instructions { get; set; } = string.Empty;
        [Display(Name = "Tài nguyên")]
        public string Resources { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        [Display(Name = "Lớp học")]
        public int ClassId { get; set; }
        [Display(Name = "Buổi học (nếu có)")]
        public int? LessonId { get; set; }
        public List<LessonItem> Lessons { get; set; } = new();
        public List<ClassroomItem> Classes { get; set; } = new();
        public string Content { get; set; } = "";
    }
} 