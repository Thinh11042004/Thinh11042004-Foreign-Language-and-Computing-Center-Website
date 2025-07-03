//using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
//using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Financial;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using OfficeOpenXml;

//namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
//{

//    [Route("api/[controller]")]
//    [ApiController]
//    [Authorize]
//    public class DashboardApiController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public DashboardApiController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet("RevenueData")]
//        public async Task<IActionResult> GetRevenueData()
//        {
//            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
//            var revenueData = await _context.Payments
//                .Where(p => p.Status == PaymentStatus.Paid && p.PaidDate >= sixMonthsAgo)
//                .GroupBy(p => new { p.PaidDate.Value.Year, p.PaidDate.Value.Month })
//                .Select(g => new
//                {
//                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
//                    Revenue = g.Sum(p => p.Amount)
//                })
//                .OrderBy(x => x.Month)
//                .ToListAsync();

//            return Ok(new
//            {
//                labels = revenueData.Select(x => DateTime.ParseExact(x.Month, "yyyy-MM", null).ToString("MMM yyyy")),
//                values = revenueData.Select(x => x.Revenue)
//            });
//        }

//        [HttpGet("EnrollmentData")]
//        public async Task<IActionResult> GetEnrollmentData()
//        {
//            var threeMonthsAgo = DateTime.Now.AddMonths(-3);
//            var enrollmentData = await _context.Enrollments
//                .Where(e => e.EnrollmentDate >= threeMonthsAgo)
//                .GroupBy(e => new { e.EnrollmentDate.Year, e.EnrollmentDate.Month })
//                .Select(g => new
//                {
//                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
//                    Count = g.Count()
//                })
//                .OrderBy(x => x.Month)
//                .ToListAsync();

//            return Ok(new
//            {
//                labels = enrollmentData.Select(x => DateTime.ParseExact(x.Month, "yyyy-MM", null).ToString("MMM yyyy")),
//                values = enrollmentData.Select(x => x.Count)
//            });
//        }

//        [HttpGet("CourseDistribution")]
//        public async Task<IActionResult> GetCourseDistribution()
//        {
//            var languageCourses = await _context.Courses.CountAsync(c => c.Type == CourseType.Language && c.IsActive);
//            var itCourses = await _context.Courses.CountAsync(c => c.Type == CourseType.IT && c.IsActive);

//            return Ok(new
//            {
//                values = new[] { languageCourses, itCourses }
//            });
//        }

//        [HttpGet("RealtimeStats")]
//        public async Task<IActionResult> GetRealtimeStats()
//        {
//            var stats = new
//            {
//                totalStudents = await _context.Students.CountAsync(),
//                totalTeachers = await _context.Teachers.CountAsync(),
//                activeClasses = await _context.Classes.CountAsync(c => c.Status == ClassStatus.Active),
//                monthlyRevenue = await _context.Payments
//                    .Where(p => p.Status == PaymentStatus.Paid &&
//                               p.PaidDate.Value.Month == DateTime.Now.Month &&
//                               p.PaidDate.Value.Year == DateTime.Now.Year)
//                    .SumAsync(p => p.Amount),
//                recentActivity = await GetRecentActivityAsync()
//            };

//            return Ok(stats);
//        }

//        [HttpPost("Export")]
//        public async Task<IActionResult> ExportDashboardData(string format = "excel")
//        {
//            try
//            {
//                var data = await _reportService.GenerateRevenueReportAsync(
//                    DateTime.Now.AddMonths(-12), DateTime.Now);

//                switch (format.ToLower())
//                {
//                    case "excel":
//                        var excelFile = ExportToExcel(data);
//                        return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//                                   "dashboard-export.xlsx");

//                    case "pdf":
//                        var pdfFile = ExportToPdf(data);
//                        return File(pdfFile, "application/pdf", "dashboard-export.pdf");

//                    default:
//                        return BadRequest("Unsupported format");
//                }
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { error = ex.Message });
//            }
//        }

//        private async Task<List<object>> GetRecentActivityAsync()
//        {
//            var activities = new List<object>();

//            // Recent enrollments
//            var recentEnrollments = await _context.Enrollments
//                .Include(e => e.Student.User)
//                .Include(e => e.Course)
//                .OrderByDescending(e => e.EnrollmentDate)
//                .Take(3)
//                .Select(e => new
//                {
//                    title = "New Enrollment",
//                    description = $"{e.Student.User.FullName} enrolled in {e.Course.Name}",
//                    timestamp = e.EnrollmentDate.ToString("MMM dd, yyyy"),
//                    icon = "fa-user-plus",
//                    type = "success"
//                })
//                .ToListAsync();

//            activities.AddRange(recentEnrollments.Cast<object>());

//            // Recent payments
//            var recentPayments = await _context.Payments
//                .Include(p => p.Student.User)
//                .Where(p => p.Status == PaymentStatus.Paid)
//                .OrderByDescending(p => p.PaidDate)
//                .Take(2)
//                .Select(p => new
//                {
//                    title = "Payment Received",
//                    description = $"${p.Amount} from {p.Student.User.FullName}",
//                    timestamp = p.PaidDate.Value.ToString("MMM dd, yyyy"),
//                    icon = "fa-dollar-sign",
//                    type = "info"
//                })
//                .ToListAsync();

//            activities.AddRange(recentPayments.Cast<object>());

//            return activities.OrderByDescending(a => ((dynamic)a).timestamp).Take(5).ToList();
//        }

//        private byte[] ExportToExcel(RevenueReportViewModel data)
//        {
//            // Implementation using EPPlus or similar library
//            using (var package = new ExcelPackage())
//            {
//                var worksheet = package.Workbook.Worksheets.Add("Revenue Report");

//                worksheet.Cells[1, 1].Value = "Month";
//                worksheet.Cells[1, 2].Value = "Revenue";

//                for (int i = 0; i < data.MonthlyBreakdown.Count; i++)
//                {
//                    worksheet.Cells[i + 2, 1].Value = data.MonthlyBreakdown[i].MonthName;
//                    worksheet.Cells[i + 2, 2].Value = data.MonthlyBreakdown[i].Revenue;
//                }

//                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

//                return package.GetAsByteArray();
//            }
//        }

//        private byte[] ExportToPdf(RevenueReportViewModel data)
//        {
//            // Implementation using iTextSharp or similar library
//            using (var stream = new MemoryStream())
//            {
//                var document = new iTextSharp.text.Document();
//                var writer = PdfWriter.GetInstance(document, stream);

//                document.Open();
//                document.Add(new Paragraph("Revenue Report"));
//                document.Add(new Paragraph($"Period: {data.StartDate:MM/yyyy} - {data.EndDate:MM/yyyy}"));
//                document.Add(new Paragraph($"Total Revenue: ${data.TotalRevenue:N2}"));

//                var table = new PdfPTable(2);
//                table.AddCell("Month");
//                table.AddCell("Revenue");

//                foreach (var item in data.MonthlyBreakdown)
//                {
//                    table.AddCell(item.MonthName);
//                    table.AddCell($"${item.Revenue:N2}");
//                }

//                document.Add(table);
//                document.Close();

//                return stream.ToArray();
//            }
//        }
//    }
//}

