// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Class;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Dashboard;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Student;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.TeacherViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.StudentViewModel;
using ClosedXML.Excel;
using System.IO;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.Shared;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : BaseController
    {
        public TeacherController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                ILogger<TeacherController> logger)
            : base(context, userManager, logger)
        {
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                        .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var model = new TeacherMainDashboardViewModel
            {
                Teacher = teacher,
                TodayClasses = await GetTodayClassesAsync(teacher.id),
                ActiveClasses = teacher.ClassTeachers
                    .Where(ct => ct.Class.Status == ClassStatus.Active)
                    .Select(ct => ct.Class)
                    .ToList(),
                PendingAssignments = await GetPendingAssignmentsAsync(teacher.id),
                RecentRatings = await _context.TeacherRatings
                    .Include(r => r.Student.User)
                    .Include(r => r.Class.Course)
                    .Where(r => r.TeacherId == teacher.id)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> MyClasses()
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);



            var classes = await _context.ClassTeachers
                .Include(ct => ct.Class)
                    .ThenInclude(c => c.Course)
                .Include(ct => ct.Class.Room)
                .Where(ct => ct.TeacherId == teacher.id)
                .Select(ct => ct.Class)
                .ToListAsync();

            return View(classes);
        }

        public async Task<IActionResult> ClassDetails(int id)
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            var classTeacher = await _context.ClassTeachers
                .Include(ct => ct.Class)
                    .ThenInclude(c => c.Course)
                .Include(ct => ct.Class.Room)
                .Include(ct => ct.Class.Enrollments)
                    .ThenInclude(e => e.Student.User)
                .FirstOrDefaultAsync(ct => ct.ClassId == id && ct.TeacherId == teacher.id);

            if (classTeacher == null)
            {
                return NotFound();
            }

            var model = new TeacherClassDetailsViewModel
            {
                Class = classTeacher.Class,
                Students = classTeacher.Class.Enrollments
                    .Where(e => e.Status == EnrollmentStatus.Studying
                        && e.RegistrationStatus == RegistrationStatus.Enrolled
                        && e.ClassId == id)
                    .Select(e => e.Student)
                    .ToList(),
                Lessons = await _context.Lessons
                    .Where(l => l.ClassId == id)
                    .OrderByDescending(l => l.ScheduledDate)
                    .ToListAsync(),
                UpcomingAssignments = await _context.Assignments
                    .Where(a => a.Lesson.ClassId == id && a.DueDate > DateTime.Now)
                    .OrderBy(a => a.DueDate)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(int classId, int lessonId, List<Models.ViewModel.StudentViewModel.AttendanceRecordViewModel> records)
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin giáo viên." });
            }

            foreach (var record in records)
            {
                var existing = await _context.AttendanceRecords
                    .FirstOrDefaultAsync(a => a.ClassId == classId && a.LessonId == lessonId && a.StudentId == record.StudentId);

                if (existing != null)
                {
                    existing.Status = record.Status;
                    existing.MarkedBy = user.UserName;
                    existing.MarkedAt = DateTime.UtcNow;
                    _context.Update(existing);
                }
                else
                {
                    // Nếu là bản ghi mới
                    var attendance = new AttendanceRecord
                    {
                        StudentId = record.StudentId,
                        ClassId = classId,
                        LessonId = lessonId,
                        Status = record.Status,
                        MarkedBy = user.UserName,
                        MarkedAt = DateTime.UtcNow
                    };
                    _context.AttendanceRecords.Add(attendance);
                }
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> Gradebook()
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                        .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Bạn chưa có hồ sơ giáo viên. Vui lòng tạo hồ sơ giáo viên trước.";
                return RedirectToAction("CreateTeacherProfile");
            }

            var classes = teacher.ClassTeachers.Select(ct => ct.Class).ToList();
            
            if (!classes.Any())
            {
                TempData["InfoMessage"] = "Bạn chưa được gán vào lớp học nào. Vui lòng liên hệ quản trị viên để được gán lớp học.";
            }

            var model = new Models.ViewModel.TeacherViewModel.GradebookViewModel
            {
                Classes = classes
            };

            return View(model);
        }

        public async Task<IActionResult> GetGradesForClass(int classId)
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
            {
                return Json(new { success = false, message = "Không tìm thấy giáo viên. Vui lòng tạo hồ sơ giáo viên trước." });
            }

            // Kiểm tra xem giáo viên có được gán vào lớp học này không
            var classTeacher = await _context.ClassTeachers
                .FirstOrDefaultAsync(ct => ct.ClassId == classId && ct.TeacherId == teacher.id);

            if (classTeacher == null)
            {
                return Json(new { success = false, message = "Bạn không có quyền truy cập vào lớp học này." });
            }

            var classRoom = await _context.Classes
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student.User)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Assignments)
                        .ThenInclude(a => a.StudentAssignments)
                .FirstOrDefaultAsync(c => c.id == classId);

            if (classRoom == null)
            {
                return Json(new { success = false, message = "Không tìm thấy lớp học." });
            }

            var studentGrades = new List<StudentGradeViewModel>();

            foreach (var enrollment in classRoom.Enrollments.Where(e => e.Status == Models.Enums.EnrollmentStatus.Studying))
            {
                var studentGrade = new StudentGradeViewModel
                {
                    StudentId = enrollment.Student.id,
                    StudentName = enrollment.Student.User.FullName,
                    Assignments = new List<AssignmentGradeViewModel>()
                };

                foreach (var lesson in classRoom.Lessons)
                {
                    foreach (var assignment in lesson.Assignments)
                    {
                        var studentAssignment = assignment.StudentAssignments.FirstOrDefault(sa => sa.StudentId == enrollment.Student.id);
                        studentGrade.Assignments.Add(new AssignmentGradeViewModel
                        {
                            AssignmentId = assignment.id,
                            AssignmentTitle = assignment.Title,
                            LessonId = lesson.id,
                            LessonTitle = lesson.Title,
                            Score = (int?)studentAssignment?.Score,
                            Status = studentAssignment?.Status ?? AssignmentStatus.Pending,
                            SubmissionContent = studentAssignment?.SubmissionContent,
                            AttachedFiles = studentAssignment?.AttachedFiles,
                            SubmittedAt = studentAssignment?.SubmittedAt,
                            StudentAssignmentId = studentAssignment?.Id
                        });
                    }
                }
                studentGrades.Add(studentGrade);
            }
            return Json(new { success = true, studentGrades = studentGrades });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAssignmentScore(int studentAssignmentId, int score)
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            var studentAssignment = await _context.StudentAssignments
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Class)
                            .ThenInclude(c => c.ClassTeachers)
                .FirstOrDefaultAsync(sa => sa.Id == studentAssignmentId);

            if (studentAssignment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài nộp." });
            }

            // Kiểm tra xem giáo viên có phải là người chấm bài này không
            if (!studentAssignment.Assignment.Lesson.Class.ClassTeachers.Any(ct => ct.TeacherId == teacher.id))
            {
                return Json(new { success = false, message = "Bạn không có quyền chấm điểm cho bài tập này." });
            }

            studentAssignment.Score = score;
            studentAssignment.Status = AssignmentStatus.Graded;
            studentAssignment.GradedAt = DateTime.UtcNow;
            studentAssignment.GradedBy = user.FullName;

            _context.StudentAssignments.Update(studentAssignment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Điểm đã được cập nhật thành công." });
        }

        [HttpGet]
        public async Task<IActionResult> CreateTeacherProfile()
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher != null)
            {
                TempData["InfoMessage"] = "Bạn đã có hồ sơ giáo viên.";
                return RedirectToAction(nameof(Dashboard));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacherProfile(string employeeCode, string specialization, string subjectAreas)
        {
            var user = await GetCurrentUserAsync();
            var existingTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (existingTeacher != null)
            {
                TempData["InfoMessage"] = "Bạn đã có hồ sơ giáo viên.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (string.IsNullOrWhiteSpace(employeeCode) || string.IsNullOrWhiteSpace(specialization) || string.IsNullOrWhiteSpace(subjectAreas))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ các trường bắt buộc.");
                return View();
            }

            var newTeacher = new Teachers
            {
                UserId = user.Id,
                Specialization = specialization,
                SubjectAreas = subjectAreas,
                HourlyRate = 0, // Giá trị mặc định
                BaseSalary = 0, // Giá trị mặc định
                Qualifications = "Chưa cập nhật", // Giá trị mặc định
                ExperienceYears = 0, // Giá trị mặc định
                Bio = "Chưa cập nhật", // Giá trị mặc định
                Rating = 0, // Giá trị mặc định
                TotalRatings = 0, // Giá trị mặc định
                IsAvailable = true,
                JoinDate = DateTime.UtcNow,
                HireDate = DateTime.UtcNow,
                IsActive = true,
                User = user
            };

            _context.Teachers.Add(newTeacher);
            await _context.SaveChangesAsync();
            await LogActivityAsync("CreateTeacherProfile", "Teacher", newTeacher.id.ToString());

            TempData["SuccessMessage"] = "Hồ sơ giáo viên của bạn đã được tạo thành công!";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public async Task<IActionResult> Attendance()
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
            {
                return View(new List<Classroom>());
            }

            var classes = teacher.ClassTeachers.Select(ct => ct.Class).ToList();
            return View(classes);
        }

        public IActionResult Resources()
        {
            return View();
        }

        private async Task<List<Classroom>> GetTodayClassesAsync(int teacherId)
        {
            var today = DateTime.Today;
            var dayOfWeek = today.DayOfWeek;

            return await _context.ClassTeachers
                .Where(ct => ct.TeacherId == teacherId && ct.Class.Status == ClassStatus.Active)
                .Select(ct => ct.Class)
                .Where(c => c.Schedule.Contains(dayOfWeek.ToString()))
                .ToListAsync();
        }

        private async Task<List<StudentAssignment>> GetPendingAssignmentsAsync(int teacherId)
        {
            return await _context.StudentAssignments
                .Include(sa => sa.Assignment)
                .Include(sa => sa.Student.User)
                .Where(sa => sa.Assignment.Lesson.Class.ClassTeachers.Any(ct => ct.TeacherId == teacherId) && sa.Status == AssignmentStatus.Submitted)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsForLesson(int lessonId)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null)
            {
                return Json(new List<object>());
            }

            var enrolledStudents = await _context.Enrollments
                .Where(e => e.ClassId == lesson.ClassId && e.Status == EnrollmentStatus.Studying)
                .Select(e => new
                {
                    e.Student.id,
                    e.Student.User.FullName
                })
                .ToListAsync();

            if (!enrolledStudents.Any())
                {
                return Json(new List<object>());
        }

            var attendanceRecords = await _context.AttendanceRecords
                .Where(ar => ar.LessonId == lessonId)
                .ToDictionaryAsync(ar => ar.StudentId);

            var studentList = enrolledStudents.Select(student =>
            {
                attendanceRecords.TryGetValue(student.id, out var record);
                return new
                {
                    id = student.id,
                    fullName = student.FullName,
                    status = record?.Status ?? Models.Enums.AttendanceStatus.Present
                };
            }).ToList();

            return Json(studentList);
        }

        [HttpGet]
        public async Task<IActionResult> ExportGradesToExcel(int classId)
        {
            var classroom = await _context.Classes
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student.User)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.id == classId);
            if (classroom == null)
                return NotFound();

            var assignments = await _context.Assignments
                .Where(a => a.Lesson.ClassId == classId)
                .OrderBy(a => a.DueDate)
                .ToListAsync();

            var studentAssignments = await _context.StudentAssignments
                .Where(sa => assignments.Select(a => a.id).Contains(sa.AssignmentId))
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Grades");
                int col = 1;
                ws.Cell(1, col++).Value = "Học sinh";
                foreach (var assignment in assignments)
                {
                    ws.Cell(1, col++).Value = assignment.Title;
                }
                int row = 2;
                foreach (var enrollment in classroom.Enrollments)
                {
                    col = 1;
                    ws.Cell(row, col++).Value = enrollment.Student.User.FullName;
                    foreach (var assignment in assignments)
                    {
                        var sa = studentAssignments.FirstOrDefault(x => x.AssignmentId == assignment.id && x.StudentId == enrollment.StudentId);
                        ws.Cell(row, col++).Value = sa?.Score ?? null;
                    }
                    row++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BangDiem_{classroom.Name}.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateAssignment(int? classId)
        {
            List<ClassroomItem> classes;
            List<LessonItem> lessons = new List<LessonItem>();
            if (classId.HasValue && classId.Value > 0)
            {
                classes = await _context.Classes
                    .Where(c => c.id == classId.Value)
                    .Select(c => new ClassroomItem { Id = c.id, Name = c.Name + " (" + c.Course.Name + ")" })
                    .ToListAsync();
                lessons = await _context.Lessons
                    .Where(l => l.ClassId == classId.Value)
                    .Select(l => new LessonItem { Id = l.id, Title = l.Title })
                    .ToListAsync();
                ViewBag.SelectedClassId = classId.Value;
            }
            else
            {
                classes = await _context.Classes
                    .Select(c => new ClassroomItem { Id = c.id, Name = c.Name + " (" + c.Course.Name + ")" })
                    .ToListAsync();
                ViewBag.SelectedClassId = null;
            }
            var model = new CreateAssignmentViewModel
            {
                Classes = classes,
                Lessons = lessons,
                DueDate = DateTime.Now.AddDays(7)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(CreateAssignmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Nạp lại danh sách lớp nếu có lỗi
                model.Classes = await _context.Classes
                    .Select(c => new ClassroomItem { Id = c.id, Name = c.Name + " (" + c.Course.Name + ")" })
                    .ToListAsync();
                if (model.ClassId > 0)
                {
                    model.Lessons = await _context.Lessons
                        .Where(l => l.ClassId == model.ClassId)
                        .Select(l => new LessonItem { Id = l.id, Title = l.Title })
                        .ToListAsync();
                }
                return View(model);
            }
            // Tạo Assignment mới
            var assignment = new Models.Learning.Assignment
            {
                Title = model.Title,
                Description = model.Description,
                MaxScore = model.MaxScore,
                DueDate = model.DueDate,
                Instructions = model.Instructions,
                Resources = model.Resources,
                LessonId = model.LessonId,
                Content = ""
            };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            // Giao bài tập cho tất cả học sinh trong lớp
            var studentIds = await _context.Enrollments
                .Where(e => e.ClassId == model.ClassId)
                .Select(e => e.StudentId)
                .ToListAsync();
            foreach (var studentId in studentIds)
            {
                var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.StudentId == studentId && e.ClassId == model.ClassId);
                if (enrollment == null) continue; // Không tạo StudentAssignment nếu không có Enrollment
                _context.StudentAssignments.Add(new Models.Learning.StudentAssignment
                {
                    AssignmentId = assignment.id,
                    StudentId = studentId,
                    Enrollment = enrollment,
                    Status = Models.Enums.AssignmentStatus.Pending,
                    AttachedFiles = "",
                    SubmissionContent = "",
                    Feedback = "",
                    GradedBy = "",
                    Title = assignment.Title
                });
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tạo và giao bài tập thành công!";
            return RedirectToAction("Gradebook");
        }

        [HttpGet]
        public async Task<IActionResult> GetLessonsForClass(int classId)
        {
            var lessons = await _context.Lessons
                .Where(l => l.ClassId == classId)
                .Select(l => new { l.id, l.Title, l.ScheduledDate })
                .ToListAsync();
            return Json(lessons);
        }

        [HttpGet]
        public async Task<IActionResult> Assignments(int? classId)
        {
            var classes = await _context.Classes
                .Select(c => new ClassroomItem { Id = c.id, Name = c.Name + " (" + c.Course.Name + ")" })
                .ToListAsync();
            var assignmentsQuery = _context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Class)
                .AsQueryable();
            if (classId.HasValue && classId.Value > 0)
            {
                assignmentsQuery = assignmentsQuery.Where(a => a.Lesson.ClassId == classId.Value);
            }
            var assignments = await assignmentsQuery
                .OrderByDescending(a => a.DueDate)
                .ToListAsync();
            ViewBag.Classes = classes;
            ViewBag.SelectedClassId = classId;
            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> EditAssignment(int id)
        {
            var assignment = await _context.Assignments.Include(a => a.Lesson).FirstOrDefaultAsync(a => a.id == id);
            if (assignment == null) return NotFound();
            var model = new CreateAssignmentViewModel
            {
                Title = assignment.Title,
                Description = assignment.Description,
                MaxScore = assignment.MaxScore,
                DueDate = assignment.DueDate,
                Instructions = assignment.Instructions,
                Resources = assignment.Resources,
                ClassId = assignment.Lesson?.ClassId ?? 0,
                LessonId = assignment.LessonId,
                Content = assignment.Content
            };
            // Nạp danh sách lớp và buổi học
            model.Classes = await _context.Classes.Select(c => new ClassroomItem { Id = c.id, Name = c.Name + " (" + c.Course.Name + ")" }).ToListAsync();
            if (model.ClassId > 0)
                model.Lessons = await _context.Lessons.Where(l => l.ClassId == model.ClassId).Select(l => new LessonItem { Id = l.id, Title = l.Title }).ToListAsync();
            ViewBag.AssignmentId = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssignment(int id, CreateAssignmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Classes = await _context.Classes.Select(c => new ClassroomItem { Id = c.id, Name = c.Name + " (" + c.Course.Name + ")" }).ToListAsync();
                if (model.ClassId > 0)
                    model.Lessons = await _context.Lessons.Where(l => l.ClassId == model.ClassId).Select(l => new LessonItem { Id = l.id, Title = l.Title }).ToListAsync();
                ViewBag.AssignmentId = id;
                return View(model);
            }
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound();
            assignment.Title = model.Title;
            assignment.Description = model.Description;
            assignment.MaxScore = model.MaxScore;
            assignment.DueDate = model.DueDate;
            assignment.Instructions = model.Instructions;
            assignment.Resources = model.Resources;
            assignment.LessonId = model.LessonId;
            assignment.Content = model.Content ?? "";
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật bài tập thành công!";
            return RedirectToAction("Assignments");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _context.Assignments.Include(a => a.Lesson).FirstOrDefaultAsync(a => a.id == id);
            if (assignment == null) return NotFound();
            return View(assignment);
        }

        [HttpPost, ActionName("DeleteAssignment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAssignmentConfirmed(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound();
            // Xóa các StudentAssignment liên quan
            var studentAssignments = _context.StudentAssignments.Where(sa => sa.AssignmentId == id);
            _context.StudentAssignments.RemoveRange(studentAssignments);
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa bài tập thành công!";
            return RedirectToAction("Assignments");
        }

        [HttpGet]
        public async Task<IActionResult> CreateLesson(int? classId)
        {
            var classes = await _context.Classes
                .Select(c => new ClassroomItem { Id = c.id, Name = c.Name + " (" + c.Course.Name + ")" })
                .ToListAsync();
            ViewBag.Classes = classes;
            ViewBag.SelectedClassId = classId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLesson(int classId, string title, DateTime scheduledDate)
        {
            if (string.IsNullOrWhiteSpace(title) || classId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                return RedirectToAction("CreateLesson", new { classId });
            }
            var lesson = new Models.Learning.Lesson
            {
                Title = title,
                ClassId = classId,
                ScheduledDate = scheduledDate,
                Content = "",
               
            };
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tạo buổi học thành công!";
            return RedirectToAction("Gradebook");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ giáo viên.";
                return RedirectToAction("Dashboard");
            }
            return View(teacher);
        }

        [HttpGet]
        public async Task<IActionResult> AttendanceList(int classId, int lessonId)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null)
            {
                return NotFound();
            }
            var classInfo = await _context.Classes.FindAsync(classId);
            var attendanceRecords = await _context.AttendanceRecords
                .Where(a => a.ClassId == classId && a.LessonId == lessonId)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .ToListAsync();
            ViewBag.ClassName = classInfo?.Name;
            ViewBag.LessonDate = lesson.ScheduledDate.ToString("dd/MM/yyyy");
            return View(attendanceRecords);
        }

        [HttpGet]
        public async Task<IActionResult> Lessons()
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (teacher == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Lấy danh sách lớp mà giáo viên phụ trách
            var classes = await _context.ClassTeachers
                .Where(ct => ct.TeacherId == teacher.id)
                .Include(ct => ct.Class)
                    .ThenInclude(c => c.Lessons)
                .Select(ct => ct.Class)
                .ToListAsync();
            return View(classes);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);
            var lesson = await _context.Lessons.Include(l => l.Class).FirstOrDefaultAsync(l => l.id == id);
            if (lesson == null || teacher == null)
                return NotFound();
            // Kiểm tra quyền giáo viên
            var isMyClass = await _context.ClassTeachers.AnyAsync(ct => ct.ClassId == lesson.ClassId && ct.TeacherId == teacher.id);
            if (!isMyClass)
                return Forbid();
            return View(lesson);
        }

        [HttpPost, ActionName("DeleteLesson")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLessonConfirmed(int id)
        {
            var user = await GetCurrentUserAsync();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);
            var lesson = await _context.Lessons.Include(l => l.Class).FirstOrDefaultAsync(l => l.id == id);
            if (lesson == null || teacher == null)
                return NotFound();
            var isMyClass = await _context.ClassTeachers.AnyAsync(ct => ct.ClassId == lesson.ClassId && ct.TeacherId == teacher.id);
            if (!isMyClass)
                return Forbid();
            // Xóa các bản ghi liên quan (attendance, assignment...)
            var attendance = _context.AttendanceRecords.Where(a => a.LessonId == id);
            _context.AttendanceRecords.RemoveRange(attendance);
            var assignments = _context.Assignments.Where(a => a.LessonId == id);
            foreach (var assignment in assignments)
            {
                var studentAssignments = _context.StudentAssignments.Where(sa => sa.AssignmentId == assignment.id);
                _context.StudentAssignments.RemoveRange(studentAssignments);
            }
            _context.Assignments.RemoveRange(assignments);
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa buổi học thành công!";
            return RedirectToAction("Lessons");
        }
    }
}
