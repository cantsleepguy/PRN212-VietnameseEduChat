using Microsoft.AspNetCore.Identity;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Users;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class UserManagementService : IUserManagementService
    {
        private static readonly HashSet<string> AllowedRoles =
            new(StringComparer.Ordinal)
            {
                AppRoles.Student,
                AppRoles.Lecturer,
                AppRoles.AcademicAdmin,
                AppRoles.SystemAdmin
            };

        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserManagementService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<UserManagementItemDto>>
            GetUsersAsync(
                string? keyword = null,
                string? roleName = null,
                bool? isLocked = null)
        {
            var users = await _userRepository.GetAllAsync(
                keyword,
                roleName,
                isLocked);

            return users.Select(x =>
                new UserManagementItemDto
                {
                    UserId = x.UserId,
                    FullName = x.FullName,
                    Email = x.Email,
                    RoleName = x.Role?.RoleName
                               ?? "Không xác định",
                    IsLocked = x.IsLocked
                })
                .ToList();
        }

        public async Task<List<string>> GetRoleNamesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();

            return roles
                .Where(x => AllowedRoles.Contains(x.RoleName))
                .Select(x => x.RoleName)
                .ToList();
        }

        public async Task CreateUserAsync(
            string fullName,
            string email,
            string password,
            string roleName)
        {
            fullName = fullName?.Trim() ?? string.Empty;
            email = email?.Trim().ToLowerInvariant()
                    ?? string.Empty;
            password ??= string.Empty;
            roleName = roleName?.Trim() ?? string.Empty;

            ValidateCreateInput(
                fullName,
                email,
                password,
                roleName);

            var emailExists =
                await _userRepository.EmailExistsAsync(email);

            if (emailExists)
            {
                throw new InvalidOperationException(
                    "Email này đã được sử dụng.");
            }

            var role =
                await _roleRepository.GetByNameAsync(roleName);

            if (role == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy role đã chọn.");
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                RoleId = role.RoleId,
                IsLocked = false
            };

            user.Password = _passwordHasher.HashPassword(
                user,
                password);

            await _userRepository.AddAsync(user);
        }

        public async Task ChangeRoleAsync(
            int userId,
            string newRoleName,
            int currentAdminUserId)
        {
            newRoleName = newRoleName?.Trim()
                          ?? string.Empty;

            ValidateRoleName(newRoleName);

            if (userId == currentAdminUserId)
            {
                throw new InvalidOperationException(
                    "Bạn không thể tự thay đổi role của chính mình.");
            }

            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy tài khoản.");
            }

            var newRole =
                await _roleRepository.GetByNameAsync(
                    newRoleName);

            if (newRole == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy role đã chọn.");
            }

            var currentRoleName = user.Role?.RoleName;

            if (currentRoleName == newRoleName)
            {
                return;
            }

            if (currentRoleName == AppRoles.SystemAdmin &&
                !user.IsLocked &&
                newRoleName != AppRoles.SystemAdmin)
            {
                await EnsureNotLastActiveSystemAdminAsync();
            }

            user.RoleId = newRole.RoleId;
            user.Role = newRole;

            await _userRepository.UpdateAsync(user);
        }

        public async Task SetLockedAsync(
            int userId,
            bool isLocked,
            int currentAdminUserId)
        {
            if (userId == currentAdminUserId)
            {
                throw new InvalidOperationException(
                    "Bạn không thể tự khóa tài khoản của chính mình.");
            }

            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy tài khoản.");
            }

            if (user.IsLocked == isLocked)
            {
                return;
            }

            if (isLocked &&
                !user.IsLocked &&
                user.Role?.RoleName == AppRoles.SystemAdmin)
            {
                await EnsureNotLastActiveSystemAdminAsync();
            }

            user.IsLocked = isLocked;

            await _userRepository.UpdateAsync(user);
        }

        private static void ValidateCreateInput(
            string fullName,
            string email,
            string password,
            string roleName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new InvalidOperationException(
                    "Họ tên không được để trống.");
            }

            if (fullName.Length > 100)
            {
                throw new InvalidOperationException(
                    "Họ tên không được dài quá 100 ký tự.");
            }

            if (string.IsNullOrWhiteSpace(email) ||
                !new EmailAddressAttribute().IsValid(email))
            {
                throw new InvalidOperationException(
                    "Email không hợp lệ.");
            }

            if (email.Length > 255)
            {
                throw new InvalidOperationException(
                    "Email không được dài quá 255 ký tự.");
            }

            if (password.Length < 6)
            {
                throw new InvalidOperationException(
                    "Mật khẩu phải có ít nhất 6 ký tự.");
            }

            ValidateRoleName(roleName);
        }

        private static void ValidateRoleName(
            string roleName)
        {
            if (!AllowedRoles.Contains(roleName))
            {
                throw new InvalidOperationException(
                    "Role không hợp lệ.");
            }
        }

        private async Task EnsureNotLastActiveSystemAdminAsync()
        {
            var activeSystemAdminCount =
                await _userRepository
                    .CountActiveByRoleNameAsync(
                        AppRoles.SystemAdmin);

            if (activeSystemAdminCount <= 1)
            {
                throw new InvalidOperationException(
                    "Không thể khóa hoặc hạ quyền System Admin đang hoạt động cuối cùng.");
            }
        }
    }
}
