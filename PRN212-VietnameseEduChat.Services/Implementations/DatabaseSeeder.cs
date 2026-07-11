using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IChunkingConfigurationService _chunkingConfigurationService;
        private readonly IPackageService _packageService;

        public DatabaseSeeder(
            ApplicationDbContext context,
            IPasswordHasher<User> hasher,
            IChunkingConfigurationService chunkingConfigurationService,
            IPackageService packageService)
        {
            _context = context;
            _hasher = hasher;
            _chunkingConfigurationService = chunkingConfigurationService;
            _packageService = packageService;
        }

        public async Task SeedAsync()
        {
            var studentRole = await EnsureRoleAsync(AppRoles.Student);
            var lecturerRole = await EnsureRoleAsync(AppRoles.Lecturer);
            var academicAdminRole = await EnsureRoleAsync(AppRoles.AcademicAdmin);
            var systemAdminRole = await EnsureRoleAsync(AppRoles.SystemAdmin);

            await EnsureUserAsync(
                "System Admin",
                "systemadmin@gmail.com",
                "123456",
                systemAdminRole);

            await EnsureUserAsync(
                "Academic Admin",
                "academicadmin@gmail.com",
                "123456",
                academicAdminRole);

            await EnsureUserAsync(
                "Nguyen Van Lecturer",
                "lecturer@gmail.com",
                "123456",
                lecturerRole);

            await EnsureUserAsync(
                "Nguyen Van Student",
                "student@gmail.com",
                "123456",
                studentRole);

            await ConvertOldCompletedDocumentsAsync();
            await EnsureDemoSubjectsAsync();
            await EnsureDemoSubjectLecturerAssignmentsAsync();

            await _chunkingConfigurationService.EnsureDefaultsAsync();
            await _packageService.EnsureDefaultsAsync();
        }

        private async Task<Role> EnsureRoleAsync(string roleName)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(x => x.RoleName == roleName);

            if (role != null)
            {
                return role;
            }

            role = new Role
            {
                RoleName = roleName
            };

            _context.Roles.Add(role);

            await _context.SaveChangesAsync();

            return role;
        }

        private async Task EnsureUserAsync(
            string fullName,
            string email,
            string password,
            Role role)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
            {
                user = new User
                {
                    FullName = fullName,
                    Email = email,
                    RoleId = role.RoleId
                };

                user.Password = _hasher.HashPassword(
                    user,
                    password);

                _context.Users.Add(user);
            }
            else
            {
                user.FullName = fullName;
                user.RoleId = role.RoleId;
                user.Password = _hasher.HashPassword(
                    user,
                    password);

                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
        }

        private async Task ConvertOldCompletedDocumentsAsync()
        {
            var oldDocuments = await _context.Documents
                .Where(x => x.Status == "Completed")
                .ToListAsync();

            foreach (var document in oldDocuments)
            {
                document.Status = "PendingApproval";
            }

            if (oldDocuments.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task EnsureDemoSubjectsAsync()
        {
            var subject = await _context.Subjects
                .Include(x => x.Chapters)
                .FirstOrDefaultAsync(x => x.SubjectName == "PRN212 - Lập trình C#");

            if (subject == null)
            {
                subject = new Subject
                {
                    SubjectName = "PRN212 - Lập trình C#",
                    Description = "Môn học lập trình C# và Razor Pages."
                };

                _context.Subjects.Add(subject);

                await _context.SaveChangesAsync();
            }

            if (!subject.Chapters.Any())
            {
                _context.Chapters.AddRange(
                    new Chapter
                    {
                        SubjectId = subject.SubjectId,
                        ChapterName = "Tổng quan môn học",
                        OrderIndex = 1
                    },
                    new Chapter
                    {
                        SubjectId = subject.SubjectId,
                        ChapterName = "Razor Pages",
                        OrderIndex = 2
                    },
                    new Chapter
                    {
                        SubjectId = subject.SubjectId,
                        ChapterName = "Entity Framework Core",
                        OrderIndex = 3
                    }
                );

                await _context.SaveChangesAsync();
            }
        }

        private async Task EnsureDemoSubjectLecturerAssignmentsAsync()
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(x =>
                    x.SubjectName == "PRN212 - Lập trình C#");

            var lecturer = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x =>
                    x.Email == "lecturer@gmail.com");

            var academicAdmin = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == "academicadmin@gmail.com");

            if (subject == null || lecturer == null || academicAdmin == null)
            {
                return;
            }

            var exists = await _context.SubjectLecturers
                .AnyAsync(x =>
                    x.SubjectId == subject.SubjectId &&
                    x.LecturerId == lecturer.UserId);

            if (exists)
            {
                return;
            }

            _context.SubjectLecturers.Add(new SubjectLecturer
            {
                SubjectId = subject.SubjectId,
                LecturerId = lecturer.UserId,
                AssignedBy = academicAdmin.UserId,
                AssignedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }
    }
}