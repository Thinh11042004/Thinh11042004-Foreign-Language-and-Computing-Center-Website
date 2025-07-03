using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Report;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Financial
{
    public class RevenueReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public Dictionary<PaymentType, decimal> PaymentsByType { get; set; } = new();
        public Dictionary<PaymentMethod, decimal> PaymentsByMethod { get; set; } = new();
        public List<MonthlyRevenueViewModel> MonthlyBreakdown { get; set; } = new();
    }
}
