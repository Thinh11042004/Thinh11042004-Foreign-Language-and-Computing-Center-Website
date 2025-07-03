// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
    using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Communication;
    using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Courses;
    using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Facilities;
    using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Feedback;

    using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
    using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Learning;
    using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Resources;
    using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.System;
    using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System.Reflection.Emit;

public class ApplicationDbContext
 : IdentityDbContext<ApplicationUser, ApplicationRole, string,
                     IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>,
                     IdentityRoleClaim<string>, IdentityUserToken<string>>

{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Identity
        //public DbSet<UserRole> UserRoles { get; set; }

        // Core Entities
        public DbSet<Teachers> Teachers { get; set; }
        public DbSet<Students> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseModule> CourseModules { get; set; }
        public DbSet<Classroom> Classes { get; set; }
        public DbSet<ClassTeacher> ClassTeachers { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }


    // Learning Management
    public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<StudentAssignment> StudentAssignments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<StudentProgress> StudentProgresses { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

     

        // Facility
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomBooking> RoomBookings { get; set; }

        // Rating & Communication
        public DbSet<TeacherRating> TeacherRatings { get; set; }
        public DbSet<TeacherSchedule> TeacherSchedules { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Resources & System
        public DbSet<Resource> Resources { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<WebsiteVisit> WebsiteVisits { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Cấu hình mối quan hệ cho Identity
        builder.Entity<UserRole>(userRole =>
        {
            userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

            userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
        });

        // Cấu hình cho ApplicationRole
        builder.Entity<ApplicationRole>()
               .Property(r => r.Description)
               .HasMaxLength(500); // Cấu hình độ dài tối đa cho mô tả

        builder.Entity<ApplicationRole>()
               .Property(r => r.CreatedAt)
               .HasDefaultValueSql("GETDATE()"); // Thiết lập mặc định cho thời gian tạo

        // Cấu hình độ chính xác cho các số thập phân
        builder.Entity<Course>()
               .Property(c => c.Fee)
               .HasPrecision(18, 2);

     

        // Cấu hình các mối quan hệ
        builder.Entity<Teachers>()
               .HasOne(t => t.User)
               .WithOne(u => u.Teacher)
               .HasForeignKey<Teachers>(t => t.UserId);

        builder.Entity<Students>()
               .HasOne(s => s.User)
               .WithOne(u => u.Student)
               .HasForeignKey<Students>(s => s.UserId);

        builder.Entity<TeacherRating>()
               .HasOne(r => r.Teacher)
               .WithMany(t => t.Ratings)
               .HasForeignKey(r => r.TeacherId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TeacherRating>()
               .HasOne(tr => tr.Student)
               .WithMany()
               .HasForeignKey(tr => tr.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TeacherRating>()
               .HasOne(tr => tr.Class)
               .WithMany()
               .HasForeignKey(tr => tr.ClassId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentAssignment>()
               .HasOne(sa => sa.Student)
               .WithMany()
               .HasForeignKey(sa => sa.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentAssignment>()
                 .HasOne(sa => sa.Assignment)
                 .WithMany(a => a.StudentAssignments)
                 .HasForeignKey(sa => sa.AssignmentId)
                 .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<StudentAssignment>()
               .HasOne(sa => sa.Enrollment)
               .WithMany()
               .HasForeignKey("EnrollmentId")
               .OnDelete(DeleteBehavior.Restrict);

        // Seed các vai trò mặc định
        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole { Id = "1", Name = "SuperAdmin", NormalizedName = "SUPERADMIN", Description = "Super Administrator", CreatedAt = DateTime.UtcNow },
            new ApplicationRole { Id = "2", Name = "Admin", NormalizedName = "ADMIN", Description = "Administrator", CreatedAt = DateTime.UtcNow  },
            new ApplicationRole { Id = "3", Name = "Teacher", NormalizedName = "TEACHER", Description = "Teacher", CreatedAt = DateTime.UtcNow },
            new ApplicationRole { Id = "4", Name = "Student", NormalizedName = "STUDENT", Description = "Student", CreatedAt = DateTime.UtcNow }
        );
    }
}
