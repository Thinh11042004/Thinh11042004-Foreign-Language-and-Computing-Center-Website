using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Report;
using System.ComponentModel.DataAnnotations;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Financial
{
    public class FinancialDashboardViewModel
    {
        [Display(Name = "Tổng doanh thu")]
        [DisplayFormat(DataFormatString = "{0:N0} VND")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Thanh toán chờ xử lý")]
        [DisplayFormat(DataFormatString = "{0:N0} VND")]
        public decimal PendingPayments { get; set; }

        [Display(Name = "Doanh thu tháng")]
        [DisplayFormat(DataFormatString = "{0:N0} VND")]
        public decimal MonthlyRevenue { get; set; }


        // Thống kê bổ sung
        [Display(Name = "Số học sinh đang học")]
        public int ActiveStudents { get; set; }

        [Display(Name = "Số thanh toán trong tháng")]
        public int MonthlyPaymentCount { get; set; }

        [Display(Name = "Tỷ lệ thanh toán đúng hạn")]
        [DisplayFormat(DataFormatString = "{0:F1}%")]
        public double OnTimePaymentRate { get; set; }

        [Display(Name = "Cập nhật lần cuối")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Dữ liệu cho biểu đồ
        public List<MonthlyRevenueViewModel> MonthlyRevenueChart { get; set; } = new List<MonthlyRevenueViewModel>();
        public List<PaymentListViewModel> PaymentStatusChart { get; set; } = new List<PaymentListViewModel>();
    }
}
