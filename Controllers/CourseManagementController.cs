using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Course;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.CourseViewModel;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Student;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.StudentViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class CourseManagementController : BaseController
    {
        public CourseManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                         ILogger<CourseManagementController> logger)
            : base(context, userManager, logger)
        {
        }

        // Courses List
        public async Task<IActionResult> Index(int page = 1, string search = "", CourseType? type = null)
        {
            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.Contains(search) );
            }

            if (type.HasValue)
            {
                query = query.Where(c => c.Type == type.Value);
            }

            var courses = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();

            // Lấy danh sách học viên cho từng khóa học
            var courseIds = courses.Select(c => c.id).ToList();
            var enrollments = await _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId))
                .Include(e => e.Student)
                .ThenInclude(s => s.User)
                .ToListAsync();
            var courseStudents = new Dictionary<int, List<Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users.Students>>();
            foreach (var course in courses)
            {
                courseStudents[course.id] = enrollments
                    .Where(e => e.CourseId == course.id)
                    .Select(e => e.Student)
                    .Where(s => s != null)
                    .ToList();
            }

            // Lấy danh sách lớp học cho từng khóa học
            var classes = await _context.Classes
                .Where(cl => courseIds.Contains(cl.CourseId))
                .Include(cl => cl.ClassTeachers)
                    .ThenInclude(ct => ct.Teacher)
                        .ThenInclude(t => t.User)
                .ToListAsync();
            var courseClasses = new Dictionary<int, List<Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses.Classroom>>();
            var courseTeachers = new Dictionary<int, List<Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users.Teachers>>();
            foreach (var course in courses)
            {
                var classList = classes.Where(cl => cl.CourseId == course.id).ToList();
                courseClasses[course.id] = classList;
                // Lấy tất cả giáo viên của các lớp học, không trùng lặp
                var teachers = classList
                    .SelectMany(cl => cl.ClassTeachers)
                    .Select(ct => ct.Teacher)
                    .Where(t => t != null)
                    .GroupBy(t => t.id)
                    .Select(g => g.First())
                    .ToList();
                courseTeachers[course.id] = teachers;
            }

            var model = new CourseListViewModel
            {
                Courses = courses,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(await query.CountAsync() / 20.0),
                SearchTerm = search,
                SelectedType = type,
                CourseStudents = courseStudents,
                CourseClasses = courseClasses,
                CourseTeachers = courseTeachers
            };

            return View(model);
        }

        // Create Course
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCourseViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = new Course
                {
                    Name = model.Name,
                    Description = model.Description,
                    Type = model.Type,
                    Category = model.Category,
                    Level = model.Level,
                    Fee = (long)model.Fee,
                    DurationHours = model.DurationHours,
                    MaxStudents = model.MaxStudents,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity.Name
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                await LogActivityAsync("CreateCourse", "Course", course.id.ToString());
                TempData["SuccessMessage"] = "Khóa học đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Course Details
        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Modules)
                .Include(c => c.Classes)
                    .ThenInclude(cl => cl.ClassTeachers)
                        .ThenInclude(ct => ct.Teacher.User)
                .FirstOrDefaultAsync(c => c.id == id);

            if (course == null)
            {
                return NotFound();
            }

            var model = new CourseDetailsViewModel
            {
                Course = course,
                TotalEnrollments = await _context.Enrollments.Where(e => e.CourseId == id).CountAsync(),
                ActiveClasses = course.Classes.Where(c => c.Status == ClassStatus.Active).Count(),
                AverageRating = await GetCourseAverageRatingAsync(id)
            };

            return View(model);
        }

       


        private async Task<double> GetCourseAverageRatingAsync(int courseId)
        {
            var classIds = await _context.Classes
                .Where(c => c.CourseId == courseId)
                .Select(c => c.id)
                .ToListAsync();

            if (!classIds.Any()) return 0;

            var ratings = await _context.TeacherRatings
                .Where(r => classIds.Contains(r.ClassId))
                .Select(r => r.Rating)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0;
        }

        // Edit Course
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var model = new CreateCourseViewModel
            {
                Name = course.Name,
                Description = course.Description,
                Type = course.Type,
                Category = course.Category,
                Level = course.Level,
                Fee = course.Fee,
                DurationHours = course.DurationHours,
                MaxStudents = course.MaxStudents
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCourseViewModel model)
        {
            if (id != model.id) // Assuming CreateCourseViewModel also has an 'id' property for editing. If not, this check needs adjustment.
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var courseToUpdate = await _context.Courses.FindAsync(id);
                    if (courseToUpdate == null)
                    {
                        return NotFound();
                    }

                    courseToUpdate.Name = model.Name;
                    courseToUpdate.Description = model.Description;
                    courseToUpdate.Type = model.Type;
                    courseToUpdate.Category = model.Category;
                    courseToUpdate.Level = model.Level;
                    courseToUpdate.Fee = (long)model.Fee;
                    courseToUpdate.DurationHours = model.DurationHours;
                    courseToUpdate.MaxStudents = model.MaxStudents;
                    courseToUpdate.IsActive = true; 

                    _context.Update(courseToUpdate);
                    await _context.SaveChangesAsync();
                    await LogActivityAsync("UpdateCourse", "Course", courseToUpdate.id.ToString());
                    TempData["SuccessMessage"] = "Khóa học đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(model.id))
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
            return View(model);
        }

        // Delete Course
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Classes)
                    .ThenInclude(cl => cl.Lessons)
                .Include(c => c.Classes)
                    .ThenInclude(cl => cl.Enrollments)
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(m => m.id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, bool? confirmDeleteAll)
        {
            var course = await _context.Courses
                .Include(c => c.Classes)
                    .ThenInclude(cl => cl.Lessons)
                        .ThenInclude(ls => ls.Assignments)
                .Include(c => c.Classes)
                    .ThenInclude(cl => cl.Enrollments)
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(c => c.id == id);
            if (course == null)
            {
                return NotFound();
            }
            // Nếu có dữ liệu liên quan thì không cho xóa, chỉ thông báo lý do
            bool hasClasses = course.Classes?.Any() == true;
            bool hasLessons = course.Classes?.Any(cl => cl.Lessons?.Any() == true) == true;
            bool hasModules = course.Modules?.Any() == true;
            if (hasClasses || hasLessons || hasModules)
            {
                string reason = "Không thể xóa khóa học này vì còn dữ liệu liên quan:";
                if (hasClasses)
                    reason += "\n- Có lớp học liên quan";
                if (hasLessons)
                    reason += "\n- Có buổi học liên quan";
                if (hasModules)
                    reason += "\n- Có module liên quan";
                ModelState.AddModelError("", reason);
                return View("Delete", course);
            }
            // Nếu không còn dữ liệu liên quan thì xóa khóa học
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Khóa học đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.id == id);
        }

        // Hiển thị danh sách học viên của một khóa học
        [HttpGet]
        [Route("CourseManagement/Students/{id}")]
        public async Task<IActionResult> Students(int id)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.id == id);
            if (course == null)
            {
                return NotFound();
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == id)
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Class)
                .ToListAsync();

            var studentEnrollment = enrollments
                .Select(e => new StudentEnrollmentViewModel
                {
                    StudentId = e.StudentId,
                    EnrollmentId = e.id,
                    FullName = e.Student.User.FullName,
                    Email = e.Student.User.Email,
                    PhoneNumber = e.Student.User.PhoneNumber,
                    EnrollmentDate = e.EnrollmentDate,
                    Status = (UserStatus)e.Status,
                    ClassName = e.Class?.Name ?? "(Chưa xếp lớp)",
                    RegistrationStatus = e.RegistrationStatus
                }).ToList();

            var model = new StudentListViewModel
            {
                Course = course,
                Students = enrollments.Select(e => e.Student).Where(s => s != null).ToList(),
                StudentEnrollment = studentEnrollment,
                CurrentPage = 1,
                TotalPages = 1,
                SearchTerm = "",
                SelectedStatus = null
            };

            return View(model);
        }


        // Thêm học viên vào khóa học
        [HttpGet]
        public async Task<IActionResult> AddStudent(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();
            var enrolledIds = await _context.Enrollments.Where(e => e.CourseId == courseId).Select(e => e.StudentId).ToListAsync();
            var students = await _context.Students.Include(s => s.User).Where(s => !enrolledIds.Contains(s.id)).ToListAsync();
            ViewBag.Course = course;
            return View(students);
        }
       


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(int courseId, int studentId)
        {
            var exists = await _context.Enrollments.AnyAsync(e => e.CourseId == courseId && e.StudentId == studentId);
            if (!exists)
            {
                var enrollment = new Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning.Enrollment
                {
                    CourseId = courseId,
                    StudentId = studentId,
                    EnrollmentDate = DateTime.Now,
                    Status = Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums.EnrollmentStatus.Studying
                };
                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Students", new { id = courseId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMultipleStudents(int courseId, List<int> selectedStudents)
        {
            if (selectedStudents != null && selectedStudents.Any())
            {
                var enrollmentsToAdd = new List<Enrollment>();
                foreach (var studentId in selectedStudents)
                {
                    var exists = await _context.Enrollments.AnyAsync(e => e.CourseId == courseId && e.StudentId == studentId);
                    if (!exists)
                    {
                        var enrollment = new Enrollment
                        {
                            CourseId = courseId,
                            StudentId = studentId,
                            EnrollmentDate = DateTime.Now,
                            Status = EnrollmentStatus.Studying
                        };
                        enrollmentsToAdd.Add(enrollment);
                    }
                }

                if (enrollmentsToAdd.Any())
                {
                    _context.Enrollments.AddRange(enrollmentsToAdd);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Students", new { id = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int enrollmentId)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
            {
                return NotFound();
            }
            enrollment.RegistrationStatus = RegistrationStatus.WaitingForClass;
            _context.Update(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Students", new { id = enrollment.CourseId });
        }




        [HttpPost]
        public async Task<IActionResult> Cancel(int enrollmentId)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
            {
                return NotFound();
            }

            enrollment.RegistrationStatus = RegistrationStatus.Cancelled;
            _context.Update(enrollment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Students", new { id = enrollment.CourseId });
        }



        // Sửa thông tin đăng ký học viên
        [HttpGet]
        [Route("CourseManagement/EditStudent/{courseId}/{studentId}")]
        public async Task<IActionResult> EditStudent(int courseId, int studentId)
        {
            var enrollment = await _context.Enrollments.Include(e => e.Student).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);
            if (enrollment == null) return NotFound();
            // Lấy danh sách lớp thuộc khóa học
            var classes = await _context.Classes.Where(c => c.CourseId == courseId).ToListAsync();
            ViewBag.Classes = new SelectList(classes, "id", "Name", enrollment.ClassId);
            return View(enrollment);
        }
        [HttpPost]
        [Route("CourseManagement/EditStudent/{courseId}/{studentId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(int courseId, int studentId, Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning.Enrollment model)
        {
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);
            if (enrollment == null) return NotFound();
            enrollment.Status = model.Status;
            enrollment.ClassId = model.ClassId;
            await _context.SaveChangesAsync();
            return RedirectToAction("Students", new { id = courseId });
        }
        // Xóa học viên khỏi khóa học
        [HttpGet("CourseManagement/DeleteStudent/{courseId}/{studentId}")]
        public async Task<IActionResult> DeleteStudent(int courseId, int studentId)
        {
            var enrollment = await _context.Enrollments.Include(e => e.Student).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);
            if (enrollment == null) return NotFound();
            return View(enrollment);
        }
        [HttpPost("CourseManagement/DeleteStudent/{courseId}/{studentId}")]
        [ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudentConfirmed(int courseId, int studentId, bool forceDelete = false)
        {
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);
            if (enrollment == null)
            {
                return NotFound();
            }
            var hasAssignments = await _context.StudentAssignments.AnyAsync(sa => sa.Enrollment.id == enrollment.id);
            if (hasAssignments && !forceDelete)
            {
                // Trả về view xác nhận xóa cứng với lý do
                ViewBag.DeleteError = "Không thể xóa học viên vì đã có bài nộp liên quan. Nếu vẫn muốn xóa, tất cả bài nộp sẽ bị xóa theo.";
                ViewBag.ForceDelete = true;
                return View("DeleteStudent", enrollment);
            }
            if (hasAssignments && forceDelete)
            {
                var assignments = _context.StudentAssignments.Where(sa => sa.Enrollment.id == enrollment.id);
                _context.StudentAssignments.RemoveRange(assignments);
            }
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Students", new { id = courseId });
        }

        [HttpGet]
        public async Task<IActionResult> AssignClass(int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Student).ThenInclude(s => s.User)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.id == enrollmentId);
            if (enrollment == null || enrollment.RegistrationStatus != RegistrationStatus.WaitingForClass)
                return NotFound();
            var availableClasses = await _context.Classes
                .Where(c => c.CourseId == enrollment.CourseId)
                .ToListAsync();
            ViewBag.Classes = new SelectList(availableClasses, "id", "Name");
            return View(enrollment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignClass(int enrollmentId, int classId)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null) return NotFound();
            enrollment.ClassId = classId;
            enrollment.RegistrationStatus = RegistrationStatus.Enrolled;
            _context.Update(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Students", new { id = enrollment.CourseId });
        }
    }
}
