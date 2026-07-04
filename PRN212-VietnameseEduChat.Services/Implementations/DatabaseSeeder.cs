using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _hasher;

        public DatabaseSeeder(
            ApplicationDbContext context,
            IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public async Task SeedAsync()
        {
            if (!_context.Roles.Any())
            {
                _context.Roles.AddRange(
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "Student" },
                    new Role { RoleName = "Lecturer" },
                    new Role { RoleName = "HeadLecturer"}
                );

                await _context.SaveChangesAsync();
            }

            if (!_context.Users.Any())
            {
                var adminRole = await _context.Roles
                    .FirstAsync(x => x.RoleName == "Admin");

                var lecturerRole = await _context.Roles
                    .FirstAsync(x => x.RoleName == "Lecturer");

                var headLecturerRole = await _context.Roles
                    .FirstAsync(x => x.RoleName == "HeadLecturer");

                var studentRole = await _context.Roles
                    .FirstAsync(x => x.RoleName == "Student");

                // Admin
                var admin = new User
                {
                    FullName = "System Admin",
                    Email = "admin@gmail.com",
                    RoleId = adminRole.RoleId
                };
                admin.Password = _hasher.HashPassword(admin, "123456");

                // Lecturer
                var lecturer = new User
                {
                    FullName = "Nguyen Van Lecturer",
                    Email = "lecturer@gmail.com",
                    RoleId = lecturerRole.RoleId
                };
                lecturer.Password = _hasher.HashPassword(lecturer, "123456");

                // Head Lecturer
                var headLecturer = new User
                {
                    FullName = "Nguyen Van Head Lecturer",
                    Email = "headlecturer@gmail.com",
                    RoleId = headLecturerRole.RoleId
                };
                headLecturer.Password = _hasher.HashPassword(headLecturer, "123456");

                // Student
                var student = new User
                {
                    FullName = "Nguyen Van Student",
                    Email = "student@gmail.com",
                    RoleId = studentRole.RoleId
                };
                student.Password = _hasher.HashPassword(student, "123456");

                _context.Users.AddRange(
                    admin,
                    lecturer,
                    student);

                await _context.SaveChangesAsync();
            }
        }
    }
}
