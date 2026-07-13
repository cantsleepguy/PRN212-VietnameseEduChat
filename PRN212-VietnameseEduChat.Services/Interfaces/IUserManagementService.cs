using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserManagementItemDto>> GetUsersAsync(
            string? keyword = null,
            string? roleName = null,
            bool? isLocked = null);

        Task<List<string>> GetRoleNamesAsync();

        Task CreateUserAsync(
            string fullName,
            string email,
            string password,
            string roleName);

        Task ChangeRoleAsync(
            int userId,
            string newRoleName,
            int currentAdminUserId);

        Task SetLockedAsync(
            int userId,
            bool isLocked,
            int currentAdminUserId);
    }
}
