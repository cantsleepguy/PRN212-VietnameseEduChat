using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByIdAsync(int id);

        Task<List<User>> GetByRoleNameAsync(string roleName);

        Task<List<User>> GetAllAsync(
            string? keyword = null,
            string? roleName = null,
            bool? isLocked = null);

        Task<bool> EmailExistsAsync(
            string email,
            int? excludeUserId = null);

        Task<int> CountActiveByRoleNameAsync(string roleName);

        Task AddAsync(User user);

        Task UpdateAsync(User user);
    }
}
