using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using System.Collections.Generic;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.StudentViewModel
{
    public class GradebookViewModel
    {
        public List<StudentAssignment> GradedAssignments { get; set; } = new();
        public List<SemesterGpaItem> GpaHistory { get; set; } = new();
        public LearningProgressItem LearningProgress { get; set; } = new();
        public string StudentName { get; set; } = string.Empty;
    }

    public class SemesterGpaItem
    {
        public string SemesterName { get; set; } = string.Empty;
        public double SemesterGpa { get; set; }
        public double CumulativeGpa { get; set; }
    }

    public class LearningProgressItem
    {
        public int TongSoBuoi { get; set; } // Tổng số buổi
        public int SoBuoiDaHoc { get; set; } // Số buổi đã học (đã điểm danh)
        public int SoBuoiChuaHoc { get; set; } // Số buổi chưa học
    }
} 