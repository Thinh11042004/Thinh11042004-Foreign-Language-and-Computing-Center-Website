using System.Globalization;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Report
{
    public class MonthlyEnrollmentViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public string MonthName => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);
    }
}
