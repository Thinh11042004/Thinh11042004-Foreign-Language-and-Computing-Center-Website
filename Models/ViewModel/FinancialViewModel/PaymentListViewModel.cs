using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Financial
{
    public class PaymentListViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public PaymentStatus? SelectedStatus { get; set; }
    }
}
