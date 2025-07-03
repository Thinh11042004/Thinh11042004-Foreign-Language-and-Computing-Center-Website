using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Resources
{
    public class Resource
    {

        public int id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ResourceType Type { get; set; }
        public string FilePath { get; set; }
        public string FileSize { get; set; }
        public string MimeType { get; set; }
        public int? CourseId { get; set; }
        public string AccessLevel { get; set; } 
        public int DownloadCount { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; }

        public virtual Courses.Course Course { get; set; }
    }
}
