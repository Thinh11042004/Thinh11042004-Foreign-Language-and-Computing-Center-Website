using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Finance
{
    public class Salary
    {
        public int id { get; set; }
        public int TeacherId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BaseSalary { get; set; }
        public int TeachingHours { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TeachingPayment { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
        public decimal TotalSalary { get; set; }
        public SalaryStatus Status { get; set; } = SalaryStatus.Calculated;
        public DateTime? PaidDate { get; set; }
        public string ProcessedBy { get; set; }
        public string Notes { get; set; }

        public virtual Teachers Teacher { get; set; }

    }
}
