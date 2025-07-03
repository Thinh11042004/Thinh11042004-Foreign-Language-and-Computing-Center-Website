using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.ReportViewModel
{
    public class ReportsIndexViewModel
    {
        public decimal TotalRevenueLast30Days { get; set; }
        public int TotalEnrollmentsLast30Days { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
    }
} 