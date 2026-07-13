using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Subscriptions;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<UserPackageInfoDto> GetUserPackageInfoAsync(int userId);

        Task<List<UserSubscription>> GetUserSubscriptionsAsync(int userId);

        Task<UserSubscription> ActivateSubscriptionAsync(
            int userId,
            int packageId);

        Task EnsureCanAskQuestionAsync(int userId);

        Task EnsureCanUploadDocumentAsync(int userId, long fileSizeBytes);

        Task<UserSubscription> ApplySuccessfulPaymentAsync(Payment payment);
    }
}
