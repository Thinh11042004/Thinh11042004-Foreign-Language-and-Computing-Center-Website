using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.UserViewModel
{
    public class CreateUserViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
        public DateTime DateOfBirth { get; set; }

        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò là bắt buộc.")]
        public string Role { get; set; } = string.Empty;

        public PasswordType PasswordType { get; set; } = PasswordType.UseEmail;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải dài ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        public string CustomPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool MustChangePassword { get; set; } = true;

        public List<string> Roles { get; set; } = new();

        public string EmergencyContact { get; set; } = string.Empty;

        // 💡 Điều kiện kiểm tra ConfirmPassword nếu dùng mật khẩu tùy chọn
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PasswordType == PasswordType.Custom)
            {
                if (string.IsNullOrWhiteSpace(CustomPassword))
                {
                    yield return new ValidationResult("Vui lòng nhập mật khẩu tùy chỉnh.", new[] { nameof(CustomPassword) });
                }

                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    yield return new ValidationResult("Vui lòng xác nhận mật khẩu.", new[] { nameof(ConfirmPassword) });
                }

                if (!string.Equals(CustomPassword, ConfirmPassword))
                {
                    yield return new ValidationResult("Mật khẩu xác nhận không khớp.", new[] { nameof(ConfirmPassword) });
                }
            }
        }
    }


}