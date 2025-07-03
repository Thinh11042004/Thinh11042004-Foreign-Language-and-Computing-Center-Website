using CourseModel = Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses.Course;
using RoomModel = Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Facilities.Room;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.ClassroomViewModel
{
    public class CreateClassViewModel
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên lớp học")]
        [Display(Name = "Tên lớp học")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khóa học")]
        [Display(Name = "Khóa học")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng học")]
        [Display(Name = "Phòng học")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }




        [Required(ErrorMessage = "Vui lòng chọn lịch học")]
        [Display(Name = "Lịch học")]
        public List<string> SelectedScheduleSlots { get; set; } = new List<string>();



        [Required(ErrorMessage = "Vui lòng nhập số học viên tối đa")]
        [Display(Name = "Số học viên tối đa")]
        [Range(1, 100, ErrorMessage = "Số học viên phải từ 1 đến 100")]
        public int MaxStudents { get; set; }

        [Display(Name = "Giáo viên")]
        public List<int> TeacherIds { get; set; }

        // Navigation properties for dropdowns
        public List<CourseModel> Courses { get; set; }
        public List<RoomModel> Rooms { get; set; }
        public List<Teachers> Teachers { get; set; }

        public CreateClassViewModel()
        {
            Courses = new List<CourseModel>();
            Rooms = new List<RoomModel>();
            Teachers = new List<Teachers>();
            SelectedScheduleSlots = new List<string>();
            TeacherIds = new List<int>();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate < StartDate)
            {
                yield return new ValidationResult(
                    "Ngày kết thúc không được nhỏ hơn ngày bắt đầu.",
                    new[] { nameof(EndDate) });
            }
        }


        public string? Schedule
        {
            get => string.Join(", ", SelectedScheduleSlots);
            set => SelectedScheduleSlots = value?.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
        }
    }




}