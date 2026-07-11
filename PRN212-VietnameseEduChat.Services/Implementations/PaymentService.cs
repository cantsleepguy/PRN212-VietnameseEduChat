using Microsoft.Extensions.Logging;
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
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPackageService _packageService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IPaymentProvider _paymentProvider;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IPackageService packageService,
            ISubscriptionService subscriptionService,
            IPaymentProvider paymentProvider,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _packageService = packageService;
            _subscriptionService = subscriptionService;
            _paymentProvider = paymentProvider;
            _logger = logger;
        }

        public async Task<string> InitiatePaymentAsync(int userId, int packageId)
        {
            var package = await _packageService.GetByIdAsync(packageId);

            if (package == null || !package.IsActive)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy gói dịch vụ hợp lệ.");
            }

            if (package.Price <= 0)
            {
                throw new InvalidOperationException(
                    "Gói miễn phí không cần thanh toán.");
            }

            var payment = new Payment
            {
                UserId = userId,
                PackageId = packageId,
                Amount = package.Price,
                Status = "Pending",
                Provider = _paymentProvider.ProviderName,
                CreatedAt = DateTime.Now
            };

            var initResult = await _paymentProvider
                .InitiatePaymentAsync(payment);

            payment.TransactionId = initResult.TransactionId;

            await _paymentRepository.AddAsync(payment);

            _logger.LogInformation(
                "Khởi tạo thanh toán {TransactionId} cho user {UserId}, gói {PackageId}, số tiền {Amount}.",
                payment.TransactionId,
                userId,
                packageId,
                payment.Amount);

            return initResult.RedirectUrl;
        }

        public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
        {
            return await _paymentRepository
                .GetByTransactionIdAsync(transactionId);
        }

        public async Task ConfirmPaymentAsync(
            string transactionId,
            int userId,
            bool isSuccessful)
        {
            var payment = await _paymentRepository
                .GetByTransactionIdAsync(transactionId);

            if (payment == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy giao dịch thanh toán.");
            }

            if (payment.UserId != userId)
            {
                throw new InvalidOperationException(
                    "Giao dịch không thuộc về người dùng hiện tại.");
            }

            if (payment.Status != "Pending")
            {
                throw new InvalidOperationException(
                    "Giao dịch đã được xử lý trước đó.");
            }

            if (isSuccessful)
            {
                payment.Status = "Success";
                payment.PaidAt = DateTime.Now;

                await _paymentRepository.UpdateAsync(payment);

                await _subscriptionService.ActivateSubscriptionAsync(
                    payment.UserId,
                    payment.PackageId);

                _logger.LogInformation(
                    "Thanh toán {TransactionId} thành công, đã kích hoạt gói {PackageId} cho user {UserId}.",
                    transactionId,
                    payment.PackageId,
                    payment.UserId);
            }
            else
            {
                payment.Status = "Failed";

                await _paymentRepository.UpdateAsync(payment);

                _logger.LogWarning(
                    "Thanh toán {TransactionId} thất bại/bị hủy bởi user {UserId}.",
                    transactionId,
                    payment.UserId);
            }
        }

        public async Task<List<Payment>> GetUserPaymentsAsync(int userId)
        {
            return await _paymentRepository.GetByUserAsync(userId);
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _paymentRepository.GetAllAsync();
        }
    }
}
