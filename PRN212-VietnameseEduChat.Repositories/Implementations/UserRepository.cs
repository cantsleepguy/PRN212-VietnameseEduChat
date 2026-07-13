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
            var normalizedEmail = email.Trim().ToLowerInvariant();

            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x =>
                    x.Email == normalizedEmail);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == id);
        }

        public async Task<List<User>> GetByRoleNameAsync(
            string roleName)
        {
            return await _context.Users
                .Include(x => x.Role)
                .Where(x =>
                    x.Role != null &&
                    x.Role.RoleName == roleName)
                .OrderBy(x => x.FullName)
                .ToListAsync();
        }

        public async Task<List<User>> GetAllAsync(
            string? keyword = null,
            string? roleName = null,
            bool? isLocked = null)
        {
            IQueryable<User> query = _context.Users
                .Include(x => x.Role)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>
                    x.FullName.Contains(keyword) ||
                    x.Email.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(roleName))
            {
                query = query.Where(x =>
                    x.Role != null &&
                    x.Role.RoleName == roleName);
            }

            if (isLocked.HasValue)
            {
                query = query.Where(x =>
                    x.IsLocked == isLocked.Value);
            }

            return await query
                .OrderBy(x => x.IsLocked)
                .ThenBy(x => x.FullName)
                .ThenBy(x => x.Email)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(
            string email,
            int? excludeUserId = null)
        {
            var normalizedEmail = email
                .Trim()
                .ToLowerInvariant();

            return await _context.Users.AnyAsync(x =>
                x.Email == normalizedEmail &&
                (!excludeUserId.HasValue ||
                 x.UserId != excludeUserId.Value));
        }

        public async Task<int> CountActiveByRoleNameAsync(
            string roleName)
        {
            return await _context.Users.CountAsync(x =>
                !x.IsLocked &&
                x.Role != null &&
                x.Role.RoleName == roleName);
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);

            await _context.SaveChangesAsync();
        }

        public async Task<HashSet<string>> GetExistingEmailsAsync(
    IEnumerable<string> emails)
        {
            var normalizedEmails = emails
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            if (normalizedEmails.Count == 0)
            {
                return new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);
            }

            var existingEmails = await _context.Users
                .AsNoTracking()
                .Where(x =>
                    normalizedEmails.Contains(x.Email.ToLower()))
                .Select(x => x.Email)
                .ToListAsync();

            return existingEmails
                .Select(x => x.Trim().ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}