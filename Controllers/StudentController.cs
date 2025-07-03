using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Course;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Dashboard;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Shared;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.StudentViewModel;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Email;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : BaseController
    {
        private readonly INotificationService _notificationService;

       
        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            INotificationService notificationService, ILogger<StudentController> logger)
            : base(context, userManager, logger)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Class)
                        .ThenInclude(c => c.ClassTeachers)
                            .ThenInclude(ct => ct.Teacher.User)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var model = new StudentDashboardViewModel
            {
                Student = student,
                ActiveEnrollments = student.Enrollments.Where(e => e.Status == EnrollmentStatus.Studying).ToList(),
                TodayClasses = await GetStudentTodayClassesAsync(student.id),
                PendingAssignments = await GetStudentPendingAssignmentsAsync(student.id),
                RecentGrades = await GetStudentRecentGradesAsync(student.id),
                OverallProgress = await GetStudentOverallProgressAsync(student.id)
            };

            return View(model);
        }

        public async Task<IActionResult> MyCourses()
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students
    .Include(s => s.User)
    .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ClassTeachers)
                        .ThenInclude(ct => ct.Teacher.User)
                .Include(e => e.Student)
                .Where(e => e.StudentId == student.id)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();

            return View(enrollments);
        }

        public async Task<IActionResult> CourseDetails(int enrollmentId)
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Modules)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ClassTeachers)
                        .ThenInclude(ct => ct.Teacher.User)
                .FirstOrDefaultAsync(e => e.id == enrollmentId && e.StudentId == student.id);

            if (enrollment == null)
            {
                return NotFound();
            }

            var model = new StudentCourseDetailsViewModel
            {
                Enrollment = enrollment,
                Progress = await _context.StudentProgresses
                    .Where(p => p.StudentId == student.id && p.CourseId == enrollment.CourseId)
                    .ToListAsync(),
                Assignments = await GetCourseAssignmentsAsync(enrollment.ClassId.Value, student.id),
                AttendanceRecords = await _context.AttendanceRecords
                    .Where(a => a.StudentId == student.id && a.ClassId == enrollment.ClassId)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Assignments()
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var assignments = await _context.StudentAssignments
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Class)
                            .ThenInclude(c => c.Course)
                .Where(sa => sa.StudentId == student.id &&
                             sa.Assignment != null &&
                             sa.Assignment.Lesson != null &&
                             sa.Assignment.Lesson.Class != null &&
                             sa.Assignment.Lesson.Class.Course != null)
                .OrderByDescending(sa => sa.Assignment.DueDate)
                .ToListAsync();

            return View(assignments);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAssignment(int assignmentId, string content, IFormFile[] files)
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null)
            {
                return NotFound();
            }

            var studentAssignment = await _context.StudentAssignments
                .FirstOrDefaultAsync(sa => sa.AssignmentId == assignmentId && sa.StudentId == student.id);

            if (studentAssignment == null)
            {
                studentAssignment = new StudentAssignment
                {
                    AssignmentId = assignmentId,
                    StudentId = student.id,
                    Status = AssignmentStatus.Submitted
                };
                _context.StudentAssignments.Add(studentAssignment);
            }

            studentAssignment.SubmissionContent = content;
            studentAssignment.SubmittedAt = DateTime.UtcNow;
            studentAssignment.Status = AssignmentStatus.Submitted;

            // Handle file uploads
            if (files != null && files.Length > 0)
            {
                var filePaths = new List<string>();
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine("uploads", "assignments", fileName);

                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        filePaths.Add(filePath);
                    }
                }
                studentAssignment.AttachedFiles = string.Join(";", filePaths);
            }

            await _context.SaveChangesAsync();
            await LogActivityAsync("SubmitAssignment", "Assignment", assignmentId.ToString());

            return RedirectToAction(nameof(Assignments));
        }

        [HttpGet]
        public async Task<IActionResult> SubmitAssignment(int assignmentId)
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null || student == null)
            {
                return NotFound();
            }
            var studentAssignment = await _context.StudentAssignments
                .FirstOrDefaultAsync(sa => sa.AssignmentId == assignmentId && sa.StudentId == student.id);
            ViewBag.Assignment = assignment;
            ViewBag.StudentAssignment = studentAssignment;
            return View();
        }

        private async Task<List<ClassroomItem>> GetStudentTodayClassesAsync(int studentId)
        {
            var today = DateTime.Today;
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Studying)
                .Include(e => e.Class)
                .ThenInclude(c => c.Course)
                .ToListAsync();

            var todayClasses = new List<ClassroomItem>();

            foreach(var enrollment in enrollments)
            {
                // This logic assumes schedule is stored as comma-separated days of week (e.g., "Monday,Wednesday,Friday")
                var scheduleDays = enrollment.Class.Schedule.Split(',').Select(s => s.Trim()).ToList();
                if (scheduleDays.Contains(today.DayOfWeek.ToString()))
                {
                    todayClasses.Add(new ClassroomItem {
                        Id = enrollment.ClassId.Value,
                        Name = enrollment.Class.Name,
                        CourseTitle = enrollment.Class.Course.Name,
                        Schedule = enrollment.Class.Schedule
                    });
                }
            }
             return todayClasses;
        }

        private async Task<List<Assignment>> GetStudentPendingAssignmentsAsync(int studentId)
        {
            return await _context.StudentAssignments
                .Where(sa => sa.StudentId == studentId && sa.Status == AssignmentStatus.Pending && sa.Assignment.DueDate > DateTime.Now)
                .Select(sa => sa.Assignment)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        private async Task<List<StudentAssignment>> GetStudentRecentGradesAsync(int studentId)
        {
            return await _context.StudentAssignments
                .Where(sa => sa.StudentId == studentId && sa.Status == AssignmentStatus.Graded)
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Class)
                            .ThenInclude(c => c.Course)
                .OrderByDescending(sa => sa.GradedAt)
                .Take(5)
                .ToListAsync();
        }

        private async Task<double> GetStudentOverallProgressAsync(int studentId)
        {
            var totalRecords = await _context.AttendanceRecords
                .CountAsync(ar => ar.StudentId == studentId);

            if (totalRecords == 0) return 100.0;

            var presentOrExcused = await _context.AttendanceRecords
                .CountAsync(ar => ar.StudentId == studentId && (ar.Status == AttendanceStatus.Present || ar.Status == AttendanceStatus.Excused));

            return (double)presentOrExcused / totalRecords * 100.0;
        }

        public async Task<IActionResult> Grades()
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var viewModel = new GradebookViewModel
            {
                StudentName = student.User.FullName,
                GradedAssignments = await GetStudentRecentGradesAsync(student.id), // Reusing this for detailed list
                LearningProgress = await GetLearningProgressAsync(student.id),
                GpaHistory = await GetGpaHistoryAsync(student.id)
            };

            return View(viewModel);
        }

        private async Task<LearningProgressItem> GetLearningProgressAsync(int studentId)
        {
            // Lấy tất cả enrollment đang học
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Studying)
                .Include(e => e.Class)
                    .ThenInclude(c => c.Lessons)
                .ToListAsync();

            // Tổng số buổi = tổng số lesson của tất cả các lớp đang học
            int tongSoBuoi = enrollments.SelectMany(e => e.Class.Lessons).Count();

            // Lấy tất cả lessonId của các lớp đang học
            var lessonIds = enrollments.SelectMany(e => e.Class.Lessons).Select(l => l.id).ToList();

            // Số buổi đã học = số lesson đã có AttendanceRecord trạng thái "Đã điểm danh" hoặc "Đã học"
            int soBuoiDaHoc = await _context.AttendanceRecords
                .Where(a => a.StudentId == studentId && a.LessonId.HasValue && lessonIds.Contains(a.LessonId.Value))
                .Select(a => a.LessonId.Value)
                .Distinct()
                .CountAsync();

            int soBuoiChuaHoc = tongSoBuoi - soBuoiDaHoc;

            return new LearningProgressItem
            {
                TongSoBuoi = tongSoBuoi,
                SoBuoiDaHoc = soBuoiDaHoc,
                SoBuoiChuaHoc = soBuoiChuaHoc
            };
        }

        private async Task<List<SemesterGpaItem>> GetGpaHistoryAsync(int studentId)
        {
            var studentAssignments = await _context.StudentAssignments
                .Where(sa => sa.StudentId == studentId && sa.Score.HasValue)
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Class)
                            .ThenInclude(c => c.Course)
                .ToListAsync();

            if (!studentAssignments.Any()) return new List<SemesterGpaItem>();
            
            const int creditsPerCourse = 3; // Temporary value

            var courseGrades = studentAssignments
                .Where(sa => sa.Assignment.Lesson?.Class?.Course != null && sa.Assignment.MaxScore > 0)
                .GroupBy(sa => sa.Assignment.Lesson.Class.Course)
                .Select(g => new
                {
                    Course = g.Key,
                    // Convert score to 4.0 scale, assuming original score is out of MaxScore
                    Gpa = g.Average(sa => (sa.Score.Value / sa.Assignment.MaxScore) * 4.0m), 
                    StartDate = g.First().Assignment.Lesson.Class.StartDate
                })
                .ToList();

            var semesterGrades = courseGrades
                .GroupBy(cg => new {
                    Year = cg.StartDate.Year,
                    // Define semesters: 1 (Jan-Apr), 2 (May-Aug), 3 (Sep-Dec)
                    Semester = (cg.StartDate.Month - 1) / 4 + 1 
                })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Semester)
                .Select(g => new
                {
                    SemesterName = $"HK{g.Key.Semester} {g.Key.Year-1}-{g.Key.Year}",
                    SemesterGpa = g.Sum(cg => cg.Gpa * creditsPerCourse) / g.Sum(cg => creditsPerCourse),
                    TotalCredits = g.Sum(cg => creditsPerCourse),
                    WeightedGpa = g.Sum(cg => cg.Gpa * creditsPerCourse)
                })
                .ToList();
            
            var history = new List<SemesterGpaItem>();
            decimal totalWeightedGpa = 0;
            int totalCreditsSoFar = 0;

            foreach (var s in semesterGrades)
            {
                totalWeightedGpa += (decimal)s.WeightedGpa;
                totalCreditsSoFar += s.TotalCredits;

                var cumulativeGpa = totalCreditsSoFar > 0 ? totalWeightedGpa / totalCreditsSoFar : 0;

                history.Add(new SemesterGpaItem
                {
                    SemesterName = s.SemesterName,
                    SemesterGpa = (double)s.SemesterGpa,
                    CumulativeGpa = (double)cumulativeGpa
                });
            }

            return history;
        }

        public async Task<IActionResult> Schedule()
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Class)
                    .ThenInclude(c => c.Course)
                .Include(e => e.Class)
                    .ThenInclude(c => c.Room)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ClassTeachers)
                        .ThenInclude(ct => ct.Teacher.User)
                .Where(e => e.StudentId == student.id && e.Status == EnrollmentStatus.Studying)
                .OrderBy(e => e.Class.StartDate)
                .ToListAsync();

            return View(enrollments);
        }

        private async Task<List<StudentAssignment>> GetCourseAssignmentsAsync(int classId, int studentId)
        {
            return await _context.StudentAssignments
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Lesson)
                .Where(sa => sa.StudentId == studentId && sa.Assignment.Lesson.ClassId == classId)
                .OrderByDescending(sa => sa.Assignment.DueDate)
                .ToListAsync();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterCourse(int CourseId, string Notes)
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin học sinh.";
                return RedirectToAction("Index", "Home");
            }

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == student.id && e.CourseId == CourseId);
            if (alreadyEnrolled)
            {
                TempData["Error"] = "Bạn đã đăng ký khóa học này rồi.";
                return RedirectToAction("MyCourses");
            }

            var enrollment = new Enrollment
            {
                StudentId = student.id,
                CourseId = CourseId,
                EnrollmentDate = DateTime.Now,
                RegistrationStatus = RegistrationStatus.Pending,
                Notes = Notes
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            await _context.Entry(enrollment).Reference(e => e.Course).LoadAsync(); 

            var adminUserId = await _context.Users
                .Where(u => u.UserRoles.Any(r => r.Role.Name == "Admin"))
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            //try
            //{
            //    await _notificationService.SendNotificationAsync(adminUserId,
            //        "Đăng ký khóa học",
            //        $"Học viên {student.User.FullName} đã đăng ký khóa học {enrollment.Course.Name}.",
            //        NotificationType.General);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error while sending notification");
            //}

            try
            {
                var message = $"Học viên {student?.User?.FullName ?? "NULL"} đã đăng ký khóa học {enrollment?.Course?.Name ?? "NULL"}";
                Console.WriteLine("📢 Gửi đến: " + adminUserId);
                Console.WriteLine("📢 Nội dung: " + message);

                await _notificationService.SendNotificationAsync(adminUserId,
                    "Đăng ký khóa học",
                    message,
                    NotificationType.General);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi gửi thông báo: " + ex.Message);
            }
            TempData["Success"] = "Đăng ký khóa học thành công, đang chờ xét duyệt.";
            return RedirectToAction("MyCourses");
        }




        public async Task<IActionResult> DetailForStudent(int id)
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

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUserAsync();
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments).ThenInclude(e => e.Class)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            return View(student);
        }
    }
}
