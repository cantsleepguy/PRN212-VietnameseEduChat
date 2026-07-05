using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == id);
        }

        public async Task<List<User>> GetByRoleNameAsync(string roleName)
        {
            return await _context.Users
                .Include(x => x.Role)
                .Where(x => x.Role != null && x.Role.RoleName == roleName)
                .OrderBy(x => x.FullName)
                .ToListAsync();
        }
    }
}
