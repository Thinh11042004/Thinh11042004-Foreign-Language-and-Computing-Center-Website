using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.System;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Traffic
{
    public interface ITrafficService
    {
        Task LogVisitAsync(HttpContext context);
        Task<List<WebsiteVisit>> GetVisitStatsAsync(DateTime startDate, DateTime endDate);
        Task<int> GetActiveUsersCountAsync();
    }
}
