using PRN212_VietnameseEduChat.BusinessObjects.Constants;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Payments;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public sealed class PaymentQuoteService : IPaymentQuoteService
    {
        private readonly IPackageService _packageService;

        private readonly IUserSubscriptionRepository
            _subscriptionRepository;

        public PaymentQuoteService(
            IPackageService packageService,
            IUserSubscriptionRepository subscriptionRepository)
        {
            _packageService = packageService;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<PaymentQuoteDto> CreateQuoteAsync(
            int userId,
            int packageId)
        {
            var targetPackage =
                await _packageService.GetByIdAsync(packageId);

            if (targetPackage == null ||
                !targetPackage.IsActive)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy gói dịch vụ hợp lệ.");
            }

            if (targetPackage.Price <= 0)
            {
                throw new InvalidOperationException(
                    "Gói miễn phí không cần thanh toán.");
            }

            if (targetPackage.DurationDays <= 0)
            {
                throw new InvalidOperationException(
                    "Thời hạn của gói dịch vụ không hợp lệ.");
            }

            var currentSubscription =
                await _subscriptionRepository
                    .GetActiveByUserAsync(userId);

            if (currentSubscription?.Package == null)
            {
                return CreateFullPurchaseQuote(targetPackage);
            }

            var currentPackage = currentSubscription.Package;

            if (currentPackage.PackageId ==
                targetPackage.PackageId)
            {
                return new PaymentQuoteDto
                {
                    TargetPackage = targetPackage,
                    SourceSubscription = currentSubscription,
                    PaymentType = PaymentTypes.Renewal,
                    GrossAmount = targetPackage.Price,
                    CreditAmount = 0,
                    Amount = decimal.Ceiling(
                        targetPackage.Price),
                    TargetEndDate = currentSubscription.EndDate
                        .AddDays(targetPackage.DurationDays)
                };
            }

            var currentRank =
                GetPackageRank(currentPackage.PackageCode);

            var targetRank =
                GetPackageRank(targetPackage.PackageCode);

            if (targetRank < currentRank)
            {
                var startDate = currentSubscription.EndDate;

                return new PaymentQuoteDto
                {
                    TargetPackage = targetPackage,
                    SourceSubscription = currentSubscription,

                    PaymentType =
                        PaymentTypes.ScheduledDowngrade,

                    GrossAmount = targetPackage.Price,
                    CreditAmount = 0,

                    Amount = decimal.Ceiling(
                        targetPackage.Price),

                    TargetStartDate = startDate,

                    TargetEndDate = startDate.AddDays(
                        targetPackage.DurationDays)
                };
            }

            if (targetRank == currentRank)
            {
                throw new InvalidOperationException(
                    "Không thể chuyển sang gói có cùng cấp độ.");
            }

            var now = DateTime.Now;

            var remainingTime =
                currentSubscription.EndDate - now;

            if (remainingTime <= TimeSpan.Zero)
            {
                return CreateFullPurchaseQuote(targetPackage);
            }

            var remainingDays =
                (decimal)remainingTime.TotalSeconds / 86400m;

            var oldDurationDays =
                Math.Max(1, currentPackage.DurationDays);

            var oldCredit =
                currentPackage.Price *
                remainingDays /
                oldDurationDays;

            var newGross =
                targetPackage.Price *
                remainingDays /
                targetPackage.DurationDays;

            oldCredit = decimal.Round(
                oldCredit,
                2,
                MidpointRounding.AwayFromZero);

            newGross = decimal.Round(
                newGross,
                2,
                MidpointRounding.AwayFromZero);

            var amount = decimal.Ceiling(
                newGross - oldCredit);

            if (amount <= 0)
            {
                throw new InvalidOperationException(
                    "Không thể tính được số tiền nâng cấp hợp lệ.");
            }

            return new PaymentQuoteDto
            {
                TargetPackage = targetPackage,
                SourceSubscription = currentSubscription,
                PaymentType = PaymentTypes.Upgrade,
                GrossAmount = newGross,
                CreditAmount = oldCredit,
                Amount = amount,
                TargetEndDate = currentSubscription.EndDate
            };
        }

        private static PaymentQuoteDto
            CreateFullPurchaseQuote(
                BusinessObjects.Entities.Package package)
        {
            return new PaymentQuoteDto
            {
                TargetPackage = package,
                PaymentType = PaymentTypes.Purchase,
                GrossAmount = package.Price,
                CreditAmount = 0,
                Amount = decimal.Ceiling(package.Price),
                TargetEndDate = DateTime.Now
                    .AddDays(package.DurationDays)
            };
        }

        private static int GetPackageRank(
            string packageCode)
        {
            return packageCode.Trim().ToLowerInvariant() switch
            {
                "free" => 0,
                "premium" => 1,
                "enterprise" => 2,

                _ => throw new InvalidOperationException(
                    $"Không xác định được cấp của gói '{packageCode}'.")
            };
        }
    }
}
