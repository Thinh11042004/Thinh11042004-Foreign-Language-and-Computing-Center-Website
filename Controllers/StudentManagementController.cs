using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Student")]
    public class StudentManagementController : BaseController
    {
        public StudentManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                          ILogger<StudentManagementController> logger)
            : base(context, userManager, logger)
        {
        }

        public async Task<IActionResult> Index(int page = 1, string search = "", UserStatus? status = null)
        {
            var query = _context.Students
                .Include(s => s.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.User.FullName.Contains(search) ||
                                        s.User.Email.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            var students = await query
                .OrderByDescending(s => s.EnrollmentDate)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();

            var model = new StudentListViewModel
            {
                Students = students,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(await query.CountAsync() / 20.0),
                SearchTerm = search,
                SelectedStatus = status
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Class)
                .Include(s => s.Progresses)
                .FirstOrDefaultAsync(s => s.id == id);

            if (student == null)
            {
                return NotFound();
            }

            var model = new StudentDetailsViewModel
            {
                Student = student,
                ActiveEnrollments = student.Enrollments.Where(e => e.Status == EnrollmentStatus.Studying).ToList(),
                AttendanceRate = await GetStudentAttendanceRateAsync(id)
            };

            return View(model);
        }

        private async Task<double> GetStudentAttendanceRateAsync(int studentId)
        {
            var totalClasses = await _context.AttendanceRecords
                .Where(a => a.StudentId == studentId)
                .CountAsync();

            if (totalClasses == 0) return 0;

            var presentClasses = await _context.AttendanceRecords
                .Where(a => a.StudentId == studentId && a.Status == AttendanceStatus.Present)
                .CountAsync();

            return (double)presentClasses / totalClasses * 100;
        }
    }

}
