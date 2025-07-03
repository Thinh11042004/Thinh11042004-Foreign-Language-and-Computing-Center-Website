using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Finance
{
    public class Payment
    {
        public int id { get; set; }
        public int StudentId { get; set; }
        public int? EnrollmentId { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal Amount { get; set; }
        public PaymentType Type { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string TransactionId { get; set; }
        public string Description { get; set; }
        public string ProcessedBy { get; set; }
        public string Notes { get; set; }

        public virtual Students Student { get; set; }
        public virtual Enrollment Enrollment { get; set; }

    }
}
