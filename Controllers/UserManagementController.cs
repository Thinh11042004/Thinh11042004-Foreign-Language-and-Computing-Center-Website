using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Filters;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.UserViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    public class UserManagementController : BaseController
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserManagementController(ApplicationDbContext context,
                                        UserManager<ApplicationUser> userManager,
                                        RoleManager<ApplicationRole> roleManager,
                                        ILogger<UserManagementController> logger)
            : base(context, userManager, logger)
        {
            _roleManager = roleManager;
        }

        [SearchUserFilter]
        [RoleFilter]
        [StatusFilter]
        public async Task<IActionResult> Index(int page = 1, string searchTerm = "", string roleFilter = "", string statusFilter = "")

        {

            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SelectedRole = roleFilter;

            string search = searchTerm?.Trim() ?? "";
            string selectedRole = roleFilter?.Trim() ?? "";
            string selectedStatus = statusFilter?.Trim() ?? "";



            var query = _userManager.Users.AsQueryable();

  
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.PhoneNumber.Contains(search));
            }

            // 🧑‍💼 Lọc theo vai trò (Role)
            if (!string.IsNullOrEmpty(selectedRole) && selectedRole != "All")
            {
                var role = await _roleManager.FindByNameAsync(selectedRole);
                if (role != null)
                {
                    var userIdsInRole = await _context.UserRoles
                        .Where(ur => ur.RoleId == role.Id)
                        .Select(ur => ur.UserId)
                        .ToListAsync();

                    query = query.Where(u => userIdsInRole.Contains(u.Id));
                }
            }

            // 🚦 Lọc theo trạng thái (Status - nếu có truyền filter)
            if (!string.IsNullOrEmpty(selectedStatus) && Enum.TryParse(selectedStatus, true, out UserStatus statusValue))
            {
                query = query.Where(u => u.Status == statusValue);
            }

            // Lấy danh sách tất cả vai trò
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            // Đếm số lượng từng vai trò
            var teacherRole = await _roleManager.FindByNameAsync("Teacher");
            var studentRole = await _roleManager.FindByNameAsync("Student");
            var adminRole = await _roleManager.FindByNameAsync("Admin");
            var superAdminRole = await _roleManager.FindByNameAsync("SuperAdmin");

            var teacherIds = teacherRole != null ? await _context.UserRoles.Where(ur => ur.RoleId == teacherRole.Id).Select(ur => ur.UserId).ToListAsync() : new List<string>();
            var studentIds = studentRole != null ? await _context.UserRoles.Where(ur => ur.RoleId == studentRole.Id).Select(ur => ur.UserId).ToListAsync() : new List<string>();
            var adminIds = adminRole != null ? await _context.UserRoles.Where(ur => ur.RoleId == adminRole.Id).Select(ur => ur.UserId).ToListAsync() : new List<string>();
            var superAdminIds = superAdminRole != null ? await _context.UserRoles.Where(ur => ur.RoleId == superAdminRole.Id).Select(ur => ur.UserId).ToListAsync() : new List<string>();

            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalTeachers = teacherIds.Count;
            ViewBag.TotalStudents = studentIds.Count;
            ViewBag.TotalAdmins = adminIds.Count + superAdminIds.Count;

            // Phân trang
            int pageSize = 20;
            var totalUsers = await query.CountAsync();
            var usersPage = await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map sang ViewModel
            var userList = new List<UsersViewModel>();
            foreach (var user in usersPage)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UsersViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    Status = user.Status,
                    CreatedAt = user.CreatedAt
                });
            }

            var model = new UserListViewModel
            {
                Users = userList,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize),
                SearchTerm = search,
                SelectedRole = selectedRole,
                SelectedStatus = selectedStatus,
                Roles = allRoles
            };

            return View(model);
        }

        // GET: UserManagement/CreateUser
        public async Task<IActionResult> CreateUser()
        {
            var model = new CreateUserViewModel
            {
                Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync()
            };
            return View(model);
        }

        // POST: UserManagement/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            _logger.LogInformation("CreateUser POST called");
            _logger.LogInformation($"Model data: FullName={model.FullName}, Email={model.Email}, Role={model.Role}, PasswordType={model.PasswordType}");

            // Log ModelState validation
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid:");
                foreach (var modelError in ModelState)
                {
                    if (modelError.Value.Errors.Count > 0)
                    {
                        _logger.LogWarning($"Field: {modelError.Key}, Errors: {string.Join(", ", modelError.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra ngày sinh hợp lệ
                if (model.DateOfBirth == DateTime.MinValue || model.DateOfBirth.Year < 1900)
                {
                    ModelState.AddModelError("DateOfBirth", "Vui lòng chọn ngày sinh hợp lệ.");
                    model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                    return View(model);
                }

                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Email already exists: {model.Email}");
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                    return View(model);
                }

                // Xác định mật khẩu sẽ sử dụng
                string passwordToUse;
                if (model.PasswordType == PasswordType.Custom && !string.IsNullOrEmpty(model.CustomPassword))
                {
                    if (model.CustomPassword != model.ConfirmPassword)
                    {
                        ModelState.AddModelError("ConfirmPassword", "Mật khẩu và xác nhận mật khẩu không khớp.");
                        return View(model);
                    }
                    passwordToUse = model.CustomPassword;
                    _logger.LogInformation("Using custom password");
                }
                else
                {
                    passwordToUse = model.Email;
                    _logger.LogInformation("Using email as password");
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    Address = model.Address,
                    CreatedAt = DateTime.UtcNow,
                    Status = UserStatus.Active,
                    MustChangePassword = model.MustChangePassword
                };

                _logger.LogInformation($"Creating user with email: {model.Email}");

                var result = await _userManager.CreateAsync(user, passwordToUse);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User created successfully. UserId: {user.Id}");

                    // Gán vai trò cho người dùng
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                        if (!roleExists)
                        {
                            _logger.LogError($"Role does not exist: {model.Role}");
                            ModelState.AddModelError("Role", "Vai trò được chọn không tồn tại.");
                            TempData["ErrorMessage"] = "Vai trò được chọn không tồn tại.";
                            await _userManager.DeleteAsync(user);
                            model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                            return View(model);
                        }

                        var addRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
                        if (!addRoleResult.Succeeded)
                        {
                            _logger.LogError($"Failed to add role {model.Role} to user {user.Id}");
                            foreach (var error in addRoleResult.Errors)
                            {
                                _logger.LogError($"Role assignment error: {error.Code} - {error.Description}");
                            }
                            ModelState.AddModelError(string.Empty, "Lỗi khi gán vai trò cho người dùng mới.");
                            TempData["ErrorMessage"] = "Lỗi khi gán vai trò cho người dùng mới.";
                            await _userManager.DeleteAsync(user);
                            model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                            return View(model);
                        }
                    }
                    else
                    {
                        var addDefaultRoleResult = await _userManager.AddToRoleAsync(user, "Student");
                        if (!addDefaultRoleResult.Succeeded)
                        {
                            _logger.LogError("Failed to add default Student role");
                            foreach (var error in addDefaultRoleResult.Errors)
                            {
                                _logger.LogError($"Default role assignment error: {error.Code} - {error.Description}");
                            }
                            ModelState.AddModelError(string.Empty, "Lỗi khi gán vai trò mặc định cho người dùng mới.");
                            TempData["ErrorMessage"] = "Lỗi khi gán vai trò mặc định cho người dùng mới.";
                            await _userManager.DeleteAsync(user);
                            model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                            return View(model);
                        }
                    }

                    try
                    {
                        // Create Student or Teacher profile if applicable
                        if (model.Role == "Student")
                        {
                            _logger.LogInformation("Creating student profile");
                            var student = new Students
                            {
                                UserId = user.Id,
                                EnrollmentDate = DateTime.Now,
                                Status = UserStatus.Active,
                                LearningGoals = "Chưa cập nhật",
                                PreviousExperience = "Chưa cập nhật"
                            };
                            _context.Students.Add(student);
                        }
                        else if (model.Role == "Teacher")
                        {
                            _logger.LogInformation("Creating teacher profile");
                            var teacher = new Teachers
                            {
                                UserId = user.Id,
                                Specialization = "Chưa cập nhật",
                                SubjectAreas = "Chưa cập nhật",
                                HourlyRate = 0,
                                BaseSalary = 0,
                                Qualifications = "Chưa cập nhật",
                                ExperienceYears = 0,
                                Bio = "Chưa cập nhật",
                                Rating = 0,
                                TotalRatings = 0,
                                IsAvailable = true,
                                JoinDate = DateTime.Now,
                                HireDate = DateTime.Now,
                                IsActive = true
                            };

                            _context.Teachers.Add(teacher);
                        }

                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Profile created and saved successfully");

                        await LogActivityAsync("CreateUser", "User", user.Id);
                        TempData["SuccessMessage"] = $"Người dùng đã được tạo thành công! Mật khẩu: {(model.PasswordType == PasswordType.Custom ? "Mật khẩu tùy chỉnh" : model.Email)}";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        string innerMsg = ex.InnerException != null ? ex.InnerException.Message : string.Empty;
                        _logger.LogError(ex, $"Lỗi khi tạo hồ sơ cho người dùng {user.Id}");
                        var errorMsg = $"Đã xảy ra lỗi khi tạo hồ sơ người dùng: {ex.Message} {(string.IsNullOrEmpty(innerMsg) ? "" : "- Chi tiết: " + innerMsg)}";
                        ModelState.AddModelError(string.Empty, errorMsg);
                        TempData["ErrorMessage"] = errorMsg;
                        await _userManager.DeleteAsync(user);
                        model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                        return View(model);
                    }
                }
                else
                {
                    _logger.LogError("Failed to create user");
                    var errorMessages = new StringBuilder();
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"User creation error: {error.Code} - {error.Description}");
                        ModelState.AddModelError(string.Empty, error.Description);
                        errorMessages.AppendLine(error.Description);
                    }
                    if (errorMessages.Length > 0)
                    {
                        TempData["ErrorMessage"] = errorMessages.ToString();
                    }
                }
            }

            _logger.LogWarning("Returning to view with errors");
            model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) { return NotFound(); }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) { return NotFound(); }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                Status = user.Status,
                SelectedRole = userRoles.FirstOrDefault(), 
                Roles = allRoles
            };
            return View(model);
        }


        // POST: UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) { return NotFound(); }

                var oldValues = System.Text.Json.JsonSerializer.Serialize(new
                {
                    user.FullName, user.Email, user.PhoneNumber, user.DateOfBirth, user.Address,
                    Status = user.Status.ToString(),
                    Roles = (await _userManager.GetRolesAsync(user)).ToList()
                });

                // Check if email is being changed and if it's already in use
                if (user.Email != model.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng bởi người dùng khác.");
                        model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                        return View(model);
                    }
                }

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email; 
                user.PhoneNumber = model.PhoneNumber;
                user.DateOfBirth = model.DateOfBirth;
                user.Address = model.Address;
                user.Status = model.Status;

                // Cập nhật EmergencyContact cho Students nếu có
                var studentProfile = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
             
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Update roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var newRole = model.SelectedRole;

                    // Get the current primary role (assuming a user typically has one primary role for profile management)
                    var currentPrimaryRole = currentRoles.FirstOrDefault(r => r == "Student" || r == "Teacher");

                    if (!string.IsNullOrEmpty(newRole) && !currentRoles.Contains(newRole))
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRoleAsync(user, newRole);

                        // Handle profile changes based on role change
                        if (currentPrimaryRole == "Student" && newRole != "Student")
                        {
                            if (studentProfile != null) _context.Students.Remove(studentProfile);
                        }
                        else if (currentPrimaryRole == "Teacher" && newRole != "Teacher")
                        {
                            var teacherProfile = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);
                            if (teacherProfile != null) _context.Teachers.Remove(teacherProfile);
                        }

                        if (newRole == "Student" && currentPrimaryRole != "Student")
                        {
                            var student = new Students
                            {
                                UserId = user.Id,
                                EnrollmentDate = DateTime.Now,
                                Status = UserStatus.Active,
                            };
                            _context.Students.Add(student);
                        }
                        else if (newRole == "Teacher" && currentPrimaryRole != "Teacher")
                        {
                            var teacher = new Teachers
                            {
                                UserId = user.Id,
                                Specialization = "Chưa cập nhật",
                                SubjectAreas = "Chưa cập nhật",
                                HourlyRate = 0,
                                BaseSalary = 0,
                                Qualifications = "Chưa cập nhật",
                                ExperienceYears = 0,
                                Bio = "Chưa cập nhật",
                                Rating = 0,
                                TotalRatings = 0,
                                IsAvailable = true,
                                JoinDate = DateTime.Now,
                                HireDate = DateTime.Now,
                                IsActive = true
                            };
                            _context.Teachers.Add(teacher);
                        }
                        await _context.SaveChangesAsync(); 
                    }
                    else if (string.IsNullOrEmpty(newRole) && currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        // And remove associated profiles
                        if (studentProfile != null) _context.Students.Remove(studentProfile);
                        var teacherProfile = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);
                        if (teacherProfile != null) _context.Teachers.Remove(teacherProfile);
                        await _context.SaveChangesAsync();
                    }

                    var newValues = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        user.FullName, user.Email, user.PhoneNumber, user.DateOfBirth, user.Address,
                        Status = user.Status.ToString(),
                        Roles = (await _userManager.GetRolesAsync(user)).ToList()
                    });

                    await LogActivityAsync("UpdateUser", "User", user.Id, oldValues, newValues);
                    TempData["SuccessMessage"] = "Người dùng đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        // GET: UserManagement/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) { return NotFound(); }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) { return NotFound(); }

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new UsersViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = userRoles.ToList(),
                Status = user.Status,
                CreatedAt = user.CreatedAt
            };
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            try
            {
                await LogActivityAsync("DeleteUser", "User", user.Id, System.Text.Json.JsonSerializer.Serialize(user), "");

                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, userRoles);
                }

                // Xóa tất cả WebsiteVisits liên quan đến user
                var websiteVisits = _context.WebsiteVisits.Where(wv => wv.UserId == user.Id);
                _context.WebsiteVisits.RemoveRange(websiteVisits);

                // Xóa dữ liệu liên quan nếu là học sinh
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == id);
                if (student != null)
                {
                    // Xóa điểm, thanh toán, điểm danh, enrollments...
                    var enrollments = _context.Enrollments.Where(e => e.StudentId == student.id);
                    var attendance = _context.AttendanceRecords.Where(a => a.StudentId == student.id);
                    var studentAssignments = _context.StudentAssignments.Where(sa => sa.StudentId == student.id);
                    var ratings = _context.TeacherRatings.Where(r => r.StudentId == student.id);

                    _context.Enrollments.RemoveRange(enrollments);
                    _context.AttendanceRecords.RemoveRange(attendance);
                    _context.StudentAssignments.RemoveRange(studentAssignments);
                    _context.TeacherRatings.RemoveRange(ratings);

                    _context.Students.Remove(student);
                }

                // Xóa dữ liệu liên quan nếu là giáo viên
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == id);
                if (teacher != null)
                {
                    var classTeachers = _context.ClassTeachers.Where(ct => ct.TeacherId == teacher.id);
                    var schedules = _context.TeacherSchedules.Where(s => s.TeacherId == teacher.id);
                    var ratings = _context.TeacherRatings.Where(r => r.TeacherId == teacher.id);

                    _context.ClassTeachers.RemoveRange(classTeachers);
                    _context.TeacherSchedules.RemoveRange(schedules);
                    _context.TeacherRatings.RemoveRange(ratings);

                    _context.Teachers.Remove(teacher);
                }

                await _context.SaveChangesAsync();

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Người dùng đã được xóa thành công!" });
                }

                // If deletion failed, return error messages
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = errors });
            }
            catch (Exception ex)
            {
                string innerMsg = ex.InnerException != null ? ex.InnerException.Message : string.Empty;
                return Json(new { success = false, message = $"Lỗi khi xóa người dùng: {ex.Message} {(string.IsNullOrEmpty(innerMsg) ? "" : "- Chi tiết: " + innerMsg)}" });
            }
        }

      
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole([FromBody] StatusUpdateRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.NewStatus))
            {
                return BadRequest(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ." });
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
            }

            var newRole = request.NewStatus; 
            var roleExists = await _roleManager.RoleExistsAsync(newRole);
            if (!roleExists)
            {
                return BadRequest(new { success = false, message = "Vai trò không tồn tại." });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    _logger.LogError("Lỗi khi xóa vai trò cũ cho người dùng {UserId}: {ErrorCode} - {Description}", user.Id, error.Code, error.Description);
                }
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa vai trò cũ." });
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    _logger.LogError("Lỗi khi gán vai trò mới '{NewRole}' cho người dùng {UserId}: {ErrorCode} - {Description}", newRole, user.Id, error.Code, error.Description);
                }
                return StatusCode(500, new { success = false, message = "Lỗi khi gán vai trò mới." });
            }

            await LogActivityAsync("AssignRole", "User", user.Id, string.Join(",", currentRoles), newRole);

            return Ok(new { success = true, message = "Gán vai trò thành công!" });
        }

        // GET: UserManagement/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new UsersViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = userRoles.ToList(),
                Status = user.Status,
                CreatedAt = user.CreatedAt
            };
            return View(model);
        }
    }
} 