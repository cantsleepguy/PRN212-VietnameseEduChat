using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.Constants;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Subscriptions;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
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
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSubscriptionRepository _subscriptionRepository;
        private readonly IPackageService _packageService;

        public SubscriptionService(
            ApplicationDbContext context,
            IUserSubscriptionRepository subscriptionRepository,
            IPackageService packageService)
        {
            _context = context;
            _subscriptionRepository = subscriptionRepository;
            _packageService = packageService;
        }

        public async Task<UserPackageInfoDto> GetUserPackageInfoAsync(int userId)
        {
            await ActivateDueScheduledSubscriptionAsync(userId);

            var subscription = await _subscriptionRepository
                .GetActiveByUserAsync(userId);

            Package package;
            bool isDefaultFreeTier;

            if (subscription?.Package != null)
            {
                package = subscription.Package;
                isDefaultFreeTier = false;
            }
            else
            {
                package = await _packageService.GetFreePackageAsync();
                isDefaultFreeTier = true;
            }

            var questionsUsedToday = await CountQuestionsTodayAsync(userId);

            var documentsOwned = await _context.Documents
                .CountAsync(d => d.UploadedBy == userId);

            return new UserPackageInfoDto
            {
                Package = package,
                Subscription = subscription,
                QuestionsUsedToday = questionsUsedToday,
                DocumentsOwned = documentsOwned,
                IsDefaultFreeTier = isDefaultFreeTier
            };
        }

        public async Task<List<UserSubscription>> GetUserSubscriptionsAsync(
            int userId)
        {
            return await _subscriptionRepository.GetByUserAsync(userId);
        }

        public async Task<UserSubscription> ActivateSubscriptionAsync(
            int userId,
            int packageId)
        {
            var package = await _packageService.GetByIdAsync(packageId);

            if (package == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy gói dịch vụ.");
            }

            var now = DateTime.Now;

            var currentSubscription = await _subscriptionRepository
                .GetActiveByUserAsync(userId);

            if (currentSubscription != null &&
                currentSubscription.PackageId == packageId)
            {
                currentSubscription.EndDate = currentSubscription.EndDate
                    .AddDays(package.DurationDays);

                await _subscriptionRepository.UpdateAsync(currentSubscription);

                return currentSubscription;
            }

            if (currentSubscription != null)
            {
                currentSubscription.Status = "Cancelled";

                await _subscriptionRepository.UpdateAsync(currentSubscription);
            }

            var subscription = new UserSubscription
            {
                UserId = userId,
                PackageId = packageId,
                StartDate = now,
                EndDate = package.DurationDays > 0
                    ? now.AddDays(package.DurationDays)
                    : now.AddYears(100),
                Status = "Active",
                CreatedAt = now
            };

            await _subscriptionRepository.AddAsync(subscription);

            return subscription;
        }

        public async Task EnsureCanAskQuestionAsync(int userId)
        {
            if (await IsExemptFromLimitsAsync(userId))
            {
                return;
            }

            var info = await GetUserPackageInfoAsync(userId);

            if (!info.Package.DailyQuestionLimit.HasValue)
            {
                return;
            }

            if (info.QuestionsUsedToday >= info.Package.DailyQuestionLimit.Value)
            {
                throw new InvalidOperationException(
                    $"Bạn đã dùng hết {info.Package.DailyQuestionLimit.Value} câu hỏi trong ngày " +
                    $"của {info.Package.PackageName}. Vui lòng nâng cấp gói hoặc quay lại vào ngày mai.");
            }
        }

        public async Task EnsureCanUploadDocumentAsync(
            int userId,
            long fileSizeBytes)
        {
            if (await IsExemptFromLimitsAsync(userId))
            {
                return;
            }

            var info = await GetUserPackageInfoAsync(userId);

            var maxBytes = (long)info.Package.MaxUploadSizeMb * 1024 * 1024;

            if (fileSizeBytes > maxBytes)
            {
                throw new InvalidOperationException(
                    $"{info.Package.PackageName} chỉ cho phép upload file tối đa " +
                    $"{info.Package.MaxUploadSizeMb}MB. Vui lòng nâng cấp gói để upload file lớn hơn.");
            }

            if (info.Package.MaxDocuments.HasValue &&
                info.DocumentsOwned >= info.Package.MaxDocuments.Value)
            {
                throw new InvalidOperationException(
                    $"{info.Package.PackageName} chỉ cho phép tối đa " +
                    $"{info.Package.MaxDocuments.Value} tài liệu. Vui lòng nâng cấp gói hoặc xóa bớt tài liệu cũ.");
            }
        }

        private async Task<bool> IsExemptFromLimitsAsync(int userId)
        {
            var roleName = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.Role != null ? u.Role.RoleName : null)
                .FirstOrDefaultAsync();

            return roleName == AppRoles.SystemAdmin ||
                   roleName == AppRoles.AcademicAdmin ||
                   roleName == AppRoles.Lecturer;
        }

        private async Task<int> CountQuestionsTodayAsync(int userId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.ChatMessages
                .Where(m =>
                    m.Role == "User" &&
                    m.CreatedAt >= today &&
                    m.CreatedAt < tomorrow &&
                    m.ChatSession != null &&
                    m.ChatSession.UserId == userId)
                .CountAsync();
        }

        public async Task<UserSubscription>
    ApplySuccessfulPaymentAsync(
        Payment payment)
        {
            var now = DateTime.Now;

            var currentSubscription =
                await _subscriptionRepository
                    .GetActiveByUserAsync(payment.UserId);

            if (payment.PaymentType ==
    PaymentTypes.ScheduledDowngrade)
            {
                if (currentSubscription == null ||
                    currentSubscription.UserSubscriptionId !=
                    payment.SourceSubscriptionId)
                {
                    throw new InvalidOperationException(
                        "Gói hiện tại đã thay đổi trong lúc chờ thanh toán.");
                }

                var allSubscriptions =
                    await _subscriptionRepository
                        .GetByUserAsync(payment.UserId);

                var existingScheduled =
                    allSubscriptions
                        .Where(x =>
                            x.Status ==
                            SubscriptionStatuses.Scheduled)
                        .ToList();

                foreach (var item in existingScheduled)
                {
                    item.Status =
                        SubscriptionStatuses.Cancelled;

                    await _subscriptionRepository
                        .UpdateAsync(item);
                }

                var scheduledSubscription =
                    new UserSubscription
                    {
                        UserId = payment.UserId,
                        PackageId = payment.PackageId,

                        StartDate = payment.TargetStartDate
                            ?? currentSubscription.EndDate,

                        EndDate = payment.TargetEndDate
                            ?? currentSubscription.EndDate.AddDays(
                                payment.PackageDurationDaysSnapshot),

                        Status =
                            SubscriptionStatuses.Scheduled,

                        CreatedAt = now
                    };

                await _subscriptionRepository
                    .AddAsync(scheduledSubscription);

                return scheduledSubscription;
            }

            if (payment.PaymentType == PaymentTypes.Renewal)
            {
                if (currentSubscription != null &&
                    currentSubscription.PackageId ==
                    payment.PackageId)
                {
                    var extendFrom =
                        currentSubscription.EndDate > now
                            ? currentSubscription.EndDate
                            : now;

                    currentSubscription.EndDate =
                        extendFrom.AddDays(
                            payment.PackageDurationDaysSnapshot);

                    await _subscriptionRepository
                        .UpdateAsync(currentSubscription);

                    return currentSubscription;
                }
            }

            if (payment.PaymentType == PaymentTypes.Upgrade)
            {
                if (currentSubscription == null ||
                    currentSubscription.UserSubscriptionId !=
                    payment.SourceSubscriptionId)
                {
                    throw new InvalidOperationException(
                        "Gói hiện tại đã thay đổi trong lúc chờ thanh toán. " +
                        "Giao dịch cần được quản trị viên kiểm tra.");
                }

                currentSubscription.Status = "Cancelled";

                await _subscriptionRepository
                    .UpdateAsync(currentSubscription);

                var upgradedSubscription = new UserSubscription
                {
                    UserId = payment.UserId,
                    PackageId = payment.PackageId,
                    StartDate = now,
                    EndDate = payment.TargetEndDate
                        ?? now.AddDays(
                            payment.PackageDurationDaysSnapshot),
                    Status = "Active",
                    CreatedAt = now
                };

                await _subscriptionRepository
                    .AddAsync(upgradedSubscription);

                return upgradedSubscription;
            }

            if (currentSubscription != null)
            {
                currentSubscription.Status = "Cancelled";

                await _subscriptionRepository
                    .UpdateAsync(currentSubscription);
            }

            var subscription = new UserSubscription
            {
                UserId = payment.UserId,
                PackageId = payment.PackageId,
                StartDate = now,
                EndDate = now.AddDays(
                    payment.PackageDurationDaysSnapshot),
                Status = "Active",
                CreatedAt = now
            };

            await _subscriptionRepository.AddAsync(subscription);

            return subscription;
        }

        private async Task ActivateDueScheduledSubscriptionAsync(
    int userId)
        {
            var now = DateTime.Now;

            var subscriptions =
                await _subscriptionRepository
                    .GetByUserAsync(userId);

            var expiredActiveSubscriptions =
                subscriptions
                    .Where(x =>
                        x.Status ==
                            SubscriptionStatuses.Active &&
                        x.EndDate <= now)
                    .ToList();

            foreach (var item in expiredActiveSubscriptions)
            {
                item.Status =
                    SubscriptionStatuses.Expired;

                await _subscriptionRepository
                    .UpdateAsync(item);
            }

            var dueScheduled =
                subscriptions
                    .Where(x =>
                        x.Status ==
                            SubscriptionStatuses.Scheduled &&
                        x.StartDate <= now)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();

            if (dueScheduled == null)
            {
                return;
            }

            dueScheduled.Status =
                SubscriptionStatuses.Active;

            await _subscriptionRepository
                .UpdateAsync(dueScheduled);
        }
    }
}
