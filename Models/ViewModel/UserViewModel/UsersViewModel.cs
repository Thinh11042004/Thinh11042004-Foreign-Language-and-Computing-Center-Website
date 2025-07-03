using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.UserViewModel
{
    public class UsersViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Vai trò")]
        public List<string> Roles { get; set; } = new();

        [Display(Name = "Trạng thái")]
        public UserStatus Status { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Phải đổi mật khẩu")]
        public bool MustChangePassword { get; set; }
    }
} 