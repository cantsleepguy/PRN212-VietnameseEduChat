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
    public class UserSubscriptionRepository : IUserSubscriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public UserSubscriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserSubscription?> GetActiveByUserAsync(int userId)
        {
            var now = DateTime.Now;

            return await _context.UserSubscriptions
                .Include(x => x.Package)
                .Where(x =>
                    x.UserId == userId &&
                    x.Status == "Active" &&
                    x.EndDate >= now)
                .OrderByDescending(x => x.EndDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserSubscription>> GetByUserAsync(int userId)
        {
            return await _context.UserSubscriptions
                .Include(x => x.Package)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(UserSubscription subscription)
        {
            _context.UserSubscriptions.Add(subscription);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserSubscription subscription)
        {
            _context.UserSubscriptions.Update(subscription);

            await _context.SaveChangesAsync();
        }
    }
}
