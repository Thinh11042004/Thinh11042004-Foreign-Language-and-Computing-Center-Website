namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums
{
    public enum PaymentStatus
    {
        Pending = 1,
        Paid = 2,
        Overdue = 3,
        Cancelled = 4,
        Refunded = 5
    }

    public enum PaymentType
    {
        Tuition = 1,
        Registration = 2,
        Material = 3,
        Exam = 4,
        Certificate = 5,
        Other = 6
    }

    public enum PaymentMethod
    {
        Cash = 1,
        BankTransfer = 2,
        CreditCard = 3,
        DebitCard = 4,
        EWallet = 5,
        Installment = 6
    }
}
