using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Report;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Financial
{
    public interface IFinancialService
    {
        Task<decimal> GetMonthlyRevenueAsync();
        Task<List<MonthlyRevenueViewModel>> GetRevenueChartDataAsync();
        Task CalculateMonthlySalariesAsync(int month, int year);
        Task ProcessPaymentAsync(int paymentId, PaymentMethod method, string transactionId);
    }
}
