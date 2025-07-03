using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.UserViewModel
{
    public class UserListViewModel
    {
        public List<UsersViewModel> Users { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedRole { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string SelectedStatus { get; set; } = string.Empty;
    }
}
