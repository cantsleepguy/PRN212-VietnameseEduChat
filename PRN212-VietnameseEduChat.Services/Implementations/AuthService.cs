using Microsoft.AspNetCore.Identity;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher<User> _hasher;

        public AuthService(
            IUserRepository repo,
            IRoleRepository roleRepository,
            IPasswordHasher<User> hasher)
        {
            _repo = repo;
            _roleRepository = roleRepository;
            _hasher = hasher;
        }

        public async Task<User?> LoginAsync(
            string email,
            string password)
        {
            email = email.Trim().ToLowerInvariant();

            var user = await _repo.GetByEmailAsync(email);

            if (user == null || user.IsLocked)
            {
                return null;
            }

            var result = _hasher.VerifyHashedPassword(
                user,
                user.Password,
                password);

            if (result == PasswordVerificationResult.Success)
            {
                return user;
            }

            return null;
        }

        public async Task<User> RegisterStudentAsync(
            string fullName,
            string email,
            string password)
        {
            fullName = fullName?.Trim() ?? string.Empty;
            email = email?.Trim().ToLowerInvariant()
                    ?? string.Empty;
            password ??= string.Empty;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new InvalidOperationException(
                    "Vui lòng nhập họ tên.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException(
                    "Vui lòng nhập email.");
            }

            if (password.Length < 6)
            {
                throw new InvalidOperationException(
                    "Mật khẩu phải có ít nhất 6 ký tự.");
            }

            if (await _repo.EmailExistsAsync(email))
            {
                throw new InvalidOperationException(
                    "Email này đã được sử dụng.");
            }

            var role =
                await _roleRepository.GetByNameAsync(
                    AppRoles.Student);

            if (role == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy role Student.");
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                RoleId = role.RoleId,
                Role = role,
                IsLocked = false
            };

            user.Password = _hasher.HashPassword(
                user,
                password);

            await _repo.AddAsync(user);

            return user;
        }
    }
}
