using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Interfaces
{
    public interface IUserSubscriptionRepository
    {
        Task<UserSubscription?> GetActiveByUserAsync(int userId);

        Task<List<UserSubscription>> GetByUserAsync(int userId);

        Task AddAsync(UserSubscription subscription);

        Task UpdateAsync(UserSubscription subscription);
    }
}
