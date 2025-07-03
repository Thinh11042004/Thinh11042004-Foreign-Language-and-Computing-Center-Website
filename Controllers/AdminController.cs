using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Communication;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Class;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Dashboard;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.LessonViewModel;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.StudentViewModel;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminController : BaseController
    {
        private readonly INotificationService _notificationService;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                              INotificationService notificationService,
                              ILogger<AdminController> logger)
            : base(context, userManager, logger)
        {
            _notificationService = notificationService;

        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalTeachers = await _context.Teachers.CountAsync(),
                ActiveClasses = await _context.Classes.Where(c => c.Status == ClassStatus.Active).CountAsync(),
                RecentEnrollments = await _context.Enrollments
                    .Include(e => e.Student.User)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Take(10)
                    .ToListAsync(),
                ClassSchedule = await GetTodayClassScheduleAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> PendingEnrollments()
        {
            var pendingEnrollments = await _context.Enrollments
                .Include(e => e.Student).ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Where(e => e.RegistrationStatus == RegistrationStatus.Pending)
                .ToListAsync();

            return View("PendingEnrollments", pendingEnrollments);
        }

        // PHẦN MỚI: Xếp lớp cho học viên đang chờ
        [HttpGet]
        public async Task<IActionResult> AssignClass(int id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Student).ThenInclude(s => s.User)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.id == id);

            if (enrollment == null || enrollment.RegistrationStatus != RegistrationStatus.Pending)
                return NotFound();

            var availableClasses = await _context.Classes
                .Where(c => c.CourseId == enrollment.CourseId)
                .ToListAsync();

            ViewBag.Classes = new SelectList(availableClasses, "id", "Name");
            return View("AssignClass", enrollment);
        }



        [HttpPost]
        [ActionName("AssignClass")]
        public async Task<IActionResult> AssignClassPost(int id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.id == id);

            if (enrollment == null) return NotFound();

            enrollment.RegistrationStatus = RegistrationStatus.Enrolled;
            await _context.SaveChangesAsync();

            // Gửi thông báo
            await _notificationService.SendNotificationAsync(
                   enrollment.Student.UserId,
                "Xếp lớp thành công",
                $"Bạn đã được xếp lớp cho khóa học {enrollment.Course.Name}.",
                NotificationType.Success);

            return RedirectToAction("DanhSachHocVien", new { courseId = enrollment.CourseId });
        }

        public async Task<IActionResult> DanhSachHocVien(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return NotFound();

            ViewBag.Course = course;

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Where(e => e.CourseId == courseId)
                .ToListAsync();

            var model = enrollments.Select(e => new StudentEnrollmentViewModel
            {
                Student = e.Student,
                Enrollment = e
            }).ToList();

            return View(model);
        }




        private async Task<List<ClassScheduleViewModel>> GetTodayClassScheduleAsync()
        {
            var today = DateTime.Today;
            var dayOfWeek = today.DayOfWeek;

            return await _context.Classes
                .Where(c => c.Status == ClassStatus.Active)
                .Select(c => new ClassScheduleViewModel
                {
                    ClassName = c.Name,
                    CourseTitle = c.Course.Name,
                    RoomName = c.Room.Name,
                    TeacherName = c.ClassTeachers.First(ct => ct.IsPrimary).Teacher.User.FullName,

                    Schedule = c.Schedule
                })
                .ToListAsync();
        }

        // Hiển thị danh sách buổi học cho admin
        public async Task<IActionResult> Lessons()
        {
            var classes = await _context.Classes
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.AttendanceRecords)
                .ToListAsync();
            return View(classes);
        }

        // GET: Tạo buổi học cho admin
        [HttpGet]
        public async Task<IActionResult> CreateLesson()
        {
            ViewBag.Classes = await _context.Classes.ToListAsync();
            return View();
        }

        // POST: Tạo buổi học cho admin
        [HttpPost]
        public async Task<IActionResult> CreateLesson(CreateLessonViewModel model)
        {
            ViewBag.Classes = await _context.Classes.ToListAsync();
            var classroom = await _context.Classes.FindAsync(model.ClassId);
            if (classroom == null)
            {
                ModelState.AddModelError("ClassId", "Không tìm thấy lớp học!");
                return View(model);
            }
            if (model.Date < classroom.StartDate.Date)
            {
                ModelState.AddModelError("Date", "Ngày học không được sớm hơn ngày bắt đầu của lớp!");
                return View(model);
            }
            // Kiểm tra ngày học không được trùng với buổi học khác trong cùng lớp
            bool isDuplicate = await _context.Lessons.AnyAsync(l => l.ClassId == model.ClassId && l.ScheduledDate.Date == model.Date.Date);
            if (isDuplicate)
            {
                ModelState.AddModelError("Date", "Đã có buổi học vào ngày này trong lớp. Vui lòng chọn ngày khác!");
                return View(model);
            }
            if (ModelState.IsValid)
            {
                var lesson = new Models.Learning.Lesson
                {
                    Title = model.Title,
                    ScheduledDate = model.Date,
                    ClassId = model.ClassId,
                    Content = string.Empty,
                    Duration = TimeSpan.FromMinutes(90),
                };
                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo buổi học thành công.";
                return RedirectToAction("Lessons");
            }
            return View(model);
        }

        // GET: Xác nhận xóa buổi học
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons.Include(l => l.Assignments).FirstOrDefaultAsync(l => l.id == id);
            if (lesson == null) return NotFound();
            return View(lesson);
        }

        // POST: Xóa buổi học
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteLesson")]
        public async Task<IActionResult> DeleteLessonConfirmed(int id, bool? confirmDeleteAssignments)
        {
            var lesson = await _context.Lessons.Include(l => l.Assignments).FirstOrDefaultAsync(l => l.id == id);
            if (lesson == null) return NotFound();
            if (lesson.Assignments != null && lesson.Assignments.Any() && confirmDeleteAssignments != true)
            {
                ModelState.AddModelError("confirmDeleteAssignments", "Bạn phải xác nhận đồng ý xóa tất cả bài tập liên quan!");
                return View("DeleteLesson", lesson);
            }
            if (lesson.Assignments != null && lesson.Assignments.Any())
            {
                _context.Assignments.RemoveRange(lesson.Assignments);
            }
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa buổi học và toàn bộ bài tập liên quan.";
            return RedirectToAction("Lessons");
        }

        // GET: Sửa buổi học
        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();
            return View(lesson);
        }

        // POST: Sửa buổi học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(int id, [Bind("id,Title,ScheduledDate,ClassId,Content")] Models.Learning.Lesson lesson)
        {
            if (id != lesson.id) return NotFound();

            // Xóa lỗi validation cho các trường navigation property
            ModelState.Remove("Class");
            ModelState.Remove("Module");
            ModelState.Remove("Assignments");
            ModelState.Remove("AttendanceRecords");

            // Kiểm tra ngày học không được sớm hơn ngày bắt đầu lớp
            var classroom = await _context.Classes.FindAsync(lesson.ClassId);
            if (classroom != null && lesson.ScheduledDate.Date < classroom.StartDate.Date)
            {
                ModelState.AddModelError("ScheduledDate", "Ngày học không được sớm hơn ngày bắt đầu của lớp!");
            }
            // Kiểm tra ngày học không được trùng với buổi học khác trong cùng lớp (trừ chính nó)
            bool isDuplicate = await _context.Lessons.AnyAsync(l => l.ClassId == lesson.ClassId && l.ScheduledDate.Date == lesson.ScheduledDate.Date && l.id != lesson.id);
            if (isDuplicate)
            {
                ModelState.AddModelError("ScheduledDate", "Đã có buổi học vào ngày này trong lớp. Vui lòng chọn ngày khác!");
            }

            if (ModelState.IsValid)
            {
                _context.Update(lesson);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật buổi học thành công.";
                return RedirectToAction("Lessons");
            }
            return View(lesson);
        }


      
    }
}
