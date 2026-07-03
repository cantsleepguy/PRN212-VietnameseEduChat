using Microsoft.AspNetCore.Identity;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;
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
        private readonly IPasswordHasher<User> _hasher;

        public AuthService(
            IUserRepository repo,
            IPasswordHasher<User> hasher)
        {
            _repo = repo;
            _hasher = hasher;
        }

        public async Task<User?> LoginAsync(
            string email,
            string password)
        {
            var user = await _repo.GetByEmailAsync(email);

            if (user == null)
                return null;

            var result = _hasher.VerifyHashedPassword(
                user,
                user.Password,
                password);

            if (result == PasswordVerificationResult.Success)
                return user;

            return null;
        }
    }
}
