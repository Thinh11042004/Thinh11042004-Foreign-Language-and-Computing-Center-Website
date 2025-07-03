using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.ClassroomViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ClassManagementController : BaseController
    {
        public ClassManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                        ILogger<ClassManagementController> logger)
            : base(context, userManager, logger)
        {
        }

        public async Task<IActionResult> Index()
        {
            var classes = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Room)
                .Include(c => c.ClassTeachers)
                    .ThenInclude(ct => ct.Teacher.User)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            return View(classes);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            CreateClassViewModel model = new CreateClassViewModel(); 

            try
            {
                model.Courses = await _context.Courses.Where(c => c.IsActive).ToListAsync();
                model.Rooms = await _context.Rooms.Where(r => r.IsActive).ToListAsync();
                model.Teachers = (await _context.Teachers
                .Include(t => t.User)
                .Where(t => t.IsAvailable)
                .ToListAsync())
                .Where(t => t.User != null)
                .ToList();

                // Đảm bảo không null
                model.Courses ??= new List<Models.Courses.Course>();
                model.Rooms ??= new List<Models.Facilities.Room>();
                model.Teachers ??= new List<Models.Users.Teachers>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang tạo lớp học.");
                ModelState.AddModelError(string.Empty, "Không thể tải dữ liệu. Vui lòng thử lại sau.");

                // Nếu gặp lỗi, vẫn đảm bảo model có danh sách rỗng để View không bị NullReference
                model.Courses = model.Courses ?? new List<Models.Courses.Course>();
                model.Rooms = model.Rooms ?? new List<Models.Facilities.Room>();
                model.Teachers = model.Teachers ?? new List<Models.Users.Teachers>();
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateClassViewModel model)
        {
            if (model.SelectedScheduleSlots == null || !model.SelectedScheduleSlots.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một ca học cho lớp!");
                model.Courses = await _context.Courses.ToListAsync();
                model.Rooms = await _context.Rooms.ToListAsync();
                model.Teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                // Nạp lại danh sách cho dropdown nếu có
                model.Courses = await _context.Courses.ToListAsync();
                model.Rooms = await _context.Rooms.ToListAsync();
                model.Teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
                return View(model);
            }

            // Lấy tổng giờ khóa học
            var course = await _context.Courses.FindAsync(model.CourseId);
            int duration = course?.DurationHours ?? 0;
            double soBuoi = Math.Ceiling(duration / 1.5); // 1 buổi = 1.5 giờ

            // Kiểm tra số buổi học đã tạo cho lớp này
            int lessonCount = await _context.Lessons.CountAsync(l => l.ClassId == model.id);
            if (lessonCount >= soBuoi)
            {
                ModelState.AddModelError("", $"Số buổi học đã đạt tối đa ({soBuoi} buổi) theo thời lượng khóa học!");
                model.Courses = await _context.Courses.ToListAsync();
                model.Rooms = await _context.Rooms.ToListAsync();
                model.Teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
                return View(model);
            }

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.id == model.RoomId);
            if (room != null && model.MaxStudents > room.Capacity)
            {
                ModelState.AddModelError("MaxStudents", $"Số học viên tối đa không được vượt quá sức chứa phòng học ({room.Capacity} học viên).");
                model.Courses = await _context.Courses.ToListAsync();
                model.Rooms = await _context.Rooms.ToListAsync();
                model.Teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
                return View(model);
            }

            model.EndDate = model.StartDate.AddDays((int)soBuoi - 1);

            // Kiểm tra trùng phòng và trùng ngày bắt đầu với lớp khác
            var newSchedule = model.SelectedScheduleSlots?.ToHashSet() ?? new HashSet<string>();

            var existingClasses = await _context.Classes
                .Where(c => c.RoomId == model.RoomId && c.StartDate == model.StartDate && c.id != model.id)
                .ToListAsync();

            foreach (var cls in existingClasses)
            {
                var existingSchedule = cls.Schedule?
                    .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                    .ToHashSet() ?? new HashSet<string>();

                if (newSchedule.Overlaps(existingSchedule))
                {
                    ModelState.AddModelError("StartDate", "Lịch học bị trùng với lớp khác trong cùng phòng và ngày! Vui lòng chọn lịch hoặc phòng khác.");
                    model.Courses = await _context.Courses.ToListAsync();
                    model.Rooms = await _context.Rooms.ToListAsync();
                    model.Teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
                    return View(model);
                }
            }


            if (ModelState.IsValid)
            {
                var newClass = new Classroom
                {
                    Name = model.Name,
                    CourseId = model.CourseId,
                    RoomId = model.RoomId,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Schedule = model.Schedule,
                    MaxStudents = model.MaxStudents,
                    Status = ClassStatus.Planned
                };

                _context.Classes.Add(newClass);
                await _context.SaveChangesAsync();

                // Assign teachers
                if (model.TeacherIds != null && model.TeacherIds.Any())
                {
                    for (int i = 0; i < model.TeacherIds.Count; i++)
                    {
                        var classTeacher = new ClassTeacher
                        {
                            ClassId = newClass.id,
                            TeacherId = model.TeacherIds[i],
                            IsPrimary = i == 0, // First teacher is primary
                            AssignedDate = DateTime.UtcNow,
                        };
                        _context.ClassTeachers.Add(classTeacher);
                    }
                    await _context.SaveChangesAsync();
                }

                await LogActivityAsync("CreateClass", "Class", newClass.id.ToString());
                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi, log lỗi và hiển thị cho người dùng
            var errors = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
            ViewBag.Error = errors;

            // Reload dropdowns if validation fails
            model.Courses = await _context.Courses.ToListAsync();
            model.Rooms = await _context.Rooms.ToListAsync();
            model.Teachers = await _context.Teachers.Include(t => t.User).ToListAsync();

            return View(model);
        }

        // Class Details
        public async Task<IActionResult> Details(int id)
        {
            var classroom = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Room)
                .Include(c => c.ClassTeachers)
                    .ThenInclude(ct => ct.Teacher.User)
                .Include(c => c.Enrollments)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.id == id);

            if (classroom == null)
            {
                return NotFound();
            }

            var model = new ClassDetailsViewModel
            {
                Class = classroom,
                TotalEnrollments = classroom.Enrollments.Count(),
                TotalLessons = classroom.Lessons.Count(),
                TotalTeachers = classroom.ClassTeachers.Count()
            };

            return View(model);
        }

        // Edit Class
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroom = await _context.Classes
                .Include(c => c.ClassTeachers)
                .FirstOrDefaultAsync(c => c.id == id);

            if (classroom == null)
            {
                return NotFound();
            }

            var model = new CreateClassViewModel
            {
                id = classroom.id,
                Name = classroom.Name,
                CourseId = classroom.CourseId,
                RoomId = classroom.RoomId,
                StartDate = classroom.StartDate,
                EndDate = classroom.EndDate,
                Schedule = classroom.Schedule,
                MaxStudents = classroom.MaxStudents,
                TeacherIds = classroom.ClassTeachers.Select(ct => ct.TeacherId).ToList(),
                Courses = await _context.Courses.Where(c => c.IsActive).ToListAsync(),
                Rooms = await _context.Rooms.Where(r => r.IsActive).ToListAsync(),
                Teachers = await _context.Teachers.Include(t => t.User).Where(t => t.IsAvailable).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateClassViewModel model)
        {
            if (id != model.id)
            {
                return NotFound();
            }

            // Kiểm tra trùng phòng và ngày bắt đầu với lớp khác khi sửa
            var newSchedule = model.SelectedScheduleSlots?.ToHashSet() ?? new HashSet<string>();

            var existingClasses = await _context.Classes
                .Where(c => c.RoomId == model.RoomId && c.StartDate == model.StartDate && c.id != model.id)
                .ToListAsync();

            foreach (var cls in existingClasses)
            {
                var existingSchedule = cls.Schedule?
                    .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                    .ToHashSet() ?? new HashSet<string>();

                if (newSchedule.Overlaps(existingSchedule))
                {
                    ModelState.AddModelError("StartDate", "Lịch học bị trùng với lớp khác trong cùng phòng và ngày! Vui lòng chọn lịch hoặc phòng khác.");
                    model.Courses = await _context.Courses.ToListAsync();
                    model.Rooms = await _context.Rooms.ToListAsync();
                    model.Teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
                    return View(model);
                }
            }


            if (ModelState.IsValid)
            {
                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.id == model.RoomId);
                if (room != null && model.MaxStudents > room.Capacity)
                {
                    ModelState.AddModelError("MaxStudents", $"Số học viên tối đa không được vượt quá sức chứa phòng học ({room.Capacity} học viên).");
                    model.Courses = await _context.Courses.Where(c => c.IsActive).ToListAsync();
                    model.Rooms = await _context.Rooms.Where(r => r.IsActive).ToListAsync();
                    model.Teachers = await _context.Teachers.Include(t => t.User).Where(t => t.IsAvailable).ToListAsync();
                    return View(model);
                }

                // Lấy tổng giờ khóa học
                var course = await _context.Courses.FindAsync(model.CourseId);
                int duration = course?.DurationHours ?? 0;
                double soBuoi = Math.Ceiling(duration / 1.5); // 1 buổi = 1.5 giờ
                model.EndDate = model.StartDate.AddDays((int)soBuoi - 1);

                try
                {
                    var classToUpdate = await _context.Classes
                        .Include(c => c.ClassTeachers)
                        .FirstOrDefaultAsync(c => c.id == id);

                    if (classToUpdate == null)
                    {
                        return NotFound();
                    }

                    var oldValues = System.Text.Json.JsonSerializer.Serialize(new {
                        classToUpdate.id,
                        classToUpdate.Name,
                        classToUpdate.CourseId,
                        classToUpdate.RoomId,
                        classToUpdate.StartDate,
                        classToUpdate.EndDate,
                        classToUpdate.Schedule,
                        classToUpdate.MaxStudents
                    });

                    classToUpdate.Name = model.Name;
                    classToUpdate.CourseId = model.CourseId;
                    classToUpdate.RoomId = model.RoomId;
                    classToUpdate.StartDate = model.StartDate;
                    classToUpdate.EndDate = model.EndDate;
                    classToUpdate.Schedule = model.Schedule;
                    classToUpdate.MaxStudents = model.MaxStudents;

                    // Update teachers
                    _context.ClassTeachers.RemoveRange(classToUpdate.ClassTeachers);
                    if (model.TeacherIds != null && model.TeacherIds.Any())
                    {
                        for (int i = 0; i < model.TeacherIds.Count; i++)
                        {
                            var classTeacher = new ClassTeacher
                            {
                                ClassId = classToUpdate.id,
                                TeacherId = model.TeacherIds[i],
                                IsPrimary = i == 0,
                                AssignedDate = DateTime.UtcNow,
                            };
                            _context.ClassTeachers.Add(classTeacher);
                        }
                    }

                    _context.Update(classToUpdate);
                    await _context.SaveChangesAsync();

                    var newValues = System.Text.Json.JsonSerializer.Serialize(new {
                        classToUpdate.id,
                        classToUpdate.Name,
                        classToUpdate.CourseId,
                        classToUpdate.RoomId,
                        classToUpdate.StartDate,
                        classToUpdate.EndDate,
                        classToUpdate.Schedule,
                        classToUpdate.MaxStudents
                    });
                    await LogActivityAsync("UpdateClass", "Class", classToUpdate.id.ToString(), oldValues, newValues);
                    TempData["SuccessMessage"] = "Lớp học đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(model.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // Reload dropdowns if validation fails
            model.Courses = await _context.Courses.Where(c => c.IsActive).ToListAsync();
            model.Rooms = await _context.Rooms.Where(r => r.IsActive).ToListAsync();
            model.Teachers = await _context.Teachers.Include(t => t.User).Where(t => t.IsAvailable).ToListAsync();
            return View(model);
        }

        // Delete Class
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroom = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Room)
                .FirstOrDefaultAsync(m => m.id == id);
            if (classroom == null)
            {
                return NotFound();
            }

            return View(classroom);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classroom = await _context.Classes
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Assignments)
                        .ThenInclude(a => a.StudentAssignments)
                .Include(c => c.Enrollments)
                .Include(c => c.ClassTeachers)
                .FirstOrDefaultAsync(c => c.id == id);

            if (classroom == null)
            {
                return NotFound();
            }

            // Capture the old values before deletion for audit log
            var oldValues = System.Text.Json.JsonSerializer.Serialize(new {
                classroom.id,
                classroom.Name,
                classroom.StartDate,
                classroom.EndDate,
                classroom.Status
            });

            try
            {
                // Delete all related records in the correct order
                foreach (var lesson in classroom.Lessons)
                {
                    // Delete attendance records for each lesson
                    var attendanceRecords = await _context.AttendanceRecords
                        .Where(ar => ar.LessonId == lesson.id)
                        .ToListAsync();
                    _context.AttendanceRecords.RemoveRange(attendanceRecords);

                    // Delete student assignments for each assignment
                    foreach (var assignment in lesson.Assignments)
                    {
                        _context.StudentAssignments.RemoveRange(assignment.StudentAssignments);
                        _context.Assignments.Remove(assignment);
                    }

                    _context.Lessons.Remove(lesson);
                }

                // Delete enrollments
                _context.Enrollments.RemoveRange(classroom.Enrollments);

                // Delete class teachers
                _context.ClassTeachers.RemoveRange(classroom.ClassTeachers);

                // Finally delete the class
                _context.Classes.Remove(classroom);
                await _context.SaveChangesAsync();

                // Log the activity with old values and empty new values for deletion
                await LogActivityAsync("DeleteClass", "Class", classroom.id.ToString(), oldValues, "");
                TempData["SuccessMessage"] = "Lớp học đã được xóa thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting class {ClassId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa lớp học. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.id == id);
        }

        private string GenerateClassCode()
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var sequence = _context.Classes.Count() + 1;
            return $"CL{year}{sequence:D4}";
        }
    }
}
