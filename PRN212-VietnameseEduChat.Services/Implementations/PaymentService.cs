using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PRN212_VietnameseEduChat.BusinessObjects.Constants;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Payments;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Options;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public sealed class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentQuoteService _quoteService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IPaymentProvider _paymentProvider;
        private readonly VnPaySettings _settings;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            ApplicationDbContext context,
            IPaymentRepository paymentRepository,
            IPaymentQuoteService quoteService,
            ISubscriptionService subscriptionService,
            IPaymentProvider paymentProvider,
            IOptions<VnPaySettings> options,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _paymentRepository = paymentRepository;
            _quoteService = quoteService;
            _subscriptionService = subscriptionService;
            _paymentProvider = paymentProvider;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<string> InitiatePaymentAsync(
            int userId,
            int packageId)
        {
            var quote = await _quoteService.CreateQuoteAsync(
                userId,
                packageId);

            var package = quote.TargetPackage;
            var now = DateTime.Now;

            var payment = new Payment
            {
                UserId = userId,
                PackageId = package.PackageId,

                PaymentType = quote.PaymentType,

                SourceSubscriptionId =
                    quote.SourceSubscription?.UserSubscriptionId,

                PackagePriceSnapshot = package.Price,
                PackageCodeSnapshot = package.PackageCode,
                PackageNameSnapshot = package.PackageName,
                PackageDurationDaysSnapshot = package.DurationDays,

                GrossAmount = quote.GrossAmount,
                CreditAmount = quote.CreditAmount,
                Amount = quote.Amount,

                Currency = "VND",
                Status = PaymentStatuses.Pending,

                TransactionId = CreateTransactionId(),

                Provider = PaymentProviders.VnPay,

                TargetStartDate = quote.TargetStartDate,
                TargetEndDate = quote.TargetEndDate,

                CreatedAt = now,

                ExpiresAt = now.AddMinutes(
                    Math.Max(
                        5,
                        _settings.ExpirationMinutes))
            };

            await _paymentRepository.AddAsync(payment);

            return "/Packages/Checkout" +
                   $"?transactionId={Uri.EscapeDataString(payment.TransactionId)}";
        }

        public async Task<string> CreateVnPayUrlAsync(
            string transactionId,
            int userId,
            string? bankCode,
            string clientIpAddress,
            CancellationToken cancellationToken = default)
        {
            var payment = await _paymentRepository
                .GetByTransactionIdAsync(transactionId);

            if (payment == null || payment.UserId != userId)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy giao dịch thanh toán.");
            }

            if (payment.Status != PaymentStatuses.Pending)
            {
                throw new InvalidOperationException(
                    "Giao dịch không còn ở trạng thái chờ thanh toán.");
            }

            if (payment.ExpiresAt.HasValue &&
                payment.ExpiresAt.Value <= DateTime.Now)
            {
                payment.Status = PaymentStatuses.Expired;
                payment.FailureReason =
                    "Giao dịch đã hết thời hạn thanh toán.";

                await _paymentRepository.UpdateAsync(payment);

                throw new InvalidOperationException(
                    "Giao dịch đã hết hạn. Vui lòng tạo giao dịch mới.");
            }

            var normalizedBankCode =
                NormalizeBankCode(bankCode);

            var initResult = await _paymentProvider
                .CreatePaymentUrlAsync(
                    payment,
                    clientIpAddress,
                    normalizedBankCode,
                    cancellationToken);

            payment.CheckoutUrl = initResult.RedirectUrl;
            payment.TransferDescription = initResult.OrderInfo;
            payment.VnPayBankCode = initResult.RequestedBankCode;

            await _paymentRepository.UpdateAsync(payment);

            return initResult.RedirectUrl;
        }

        public Task<Payment?> GetByTransactionIdAsync(
            string transactionId)
        {
            return _paymentRepository
                .GetByTransactionIdAsync(transactionId);
        }

        public async Task CancelPendingPaymentAsync(
            string transactionId,
            int userId)
        {
            var payment = await _paymentRepository
                .GetByTransactionIdAsync(transactionId);

            if (payment == null || payment.UserId != userId)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy giao dịch.");
            }

            if (payment.Status != PaymentStatuses.Pending)
            {
                throw new InvalidOperationException(
                    "Giao dịch không còn ở trạng thái chờ.");
            }

            payment.Status = PaymentStatuses.Cancelled;
            payment.CancelledAt = DateTime.Now;
            payment.FailureReason =
                "Người dùng đã hủy giao dịch.";

            await _paymentRepository.UpdateAsync(payment);
        }

        public async Task<VnPayIpnResultDto> ProcessVnPayIpnAsync(
            IReadOnlyDictionary<string, string> callbackValues,
            CancellationToken cancellationToken = default)
        {
            if (!_paymentProvider.ValidateCallbackSignature(
                    callbackValues))
            {
                _logger.LogWarning(
                    "IPN VNPay có chữ ký không hợp lệ.");

                return CreateIpnResult(
                    "97",
                    "Invalid signature");
            }

            var callback =
                _paymentProvider.ParseCallback(callbackValues);

            if (string.IsNullOrWhiteSpace(
                    callback.TransactionId))
            {
                return CreateIpnResult(
                    "01",
                    "Order not found");
            }

            var payment = await _paymentRepository
                .GetByTransactionIdAsync(
                    callback.TransactionId);

            if (payment == null)
            {
                return CreateIpnResult(
                    "01",
                    "Order not found");
            }

            if (payment.Status == PaymentStatuses.Success)
            {
                return CreateIpnResult(
                    "02",
                    "Order already confirmed");
            }

            if (payment.Status != PaymentStatuses.Pending)
            {
                return CreateIpnResult(
                    "02",
                    "Order already confirmed");
            }

            ApplyVnPayCallbackData(payment, callback);

            var expectedRawAmount = checked(
                (long)decimal.Ceiling(payment.Amount) * 100L);

            if (callback.RawAmount != expectedRawAmount)
            {
                payment.Status =
                    PaymentStatuses.AmountMismatch;

                payment.FailureReason =
                    $"VNPay trả về số tiền " +
                    $"{callback.RawAmount / 100m:N0}đ, " +
                    $"trong khi giao dịch yêu cầu " +
                    $"{payment.Amount:N0}đ.";

                await _paymentRepository.UpdateAsync(payment);

                return CreateIpnResult(
                    "04",
                    "Invalid amount");
            }

            if (!callback.IsSuccessful)
            {
                payment.Status = PaymentStatuses.Failed;

                payment.FailureReason =
                    $"VNPay từ chối giao dịch. " +
                    $"ResponseCode={callback.ResponseCode}, " +
                    $"TransactionStatus={callback.TransactionStatus}.";

                await _paymentRepository.UpdateAsync(payment);

                // Đã ghi nhận kết quả thất bại, VNPay không cần gửi lại IPN.
                return CreateIpnResult(
                    "00",
                    "Confirm success");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    cancellationToken);

            try
            {
                var claimed = await _paymentRepository.TryClaimPendingAsync(
                    payment.PaymentId,
                    cancellationToken);
                if (!claimed)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return CreateIpnResult(
                        "02",
                        "Order already confirmed");
                }

                _context.ChangeTracker.Clear();
                payment = await _paymentRepository.GetByIdAsync(payment.PaymentId);
                if (payment == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return CreateIpnResult("01", "Order not found");
                }

                ApplyVnPayCallbackData(payment, callback);
                payment.Status = PaymentStatuses.Success;
                payment.PaidAt = callback.PayDate ?? DateTime.Now;
                payment.FailureReason = null;

                await _paymentRepository.UpdateAsync(payment);

                await _subscriptionService
                    .ApplySuccessfulPaymentAsync(payment);

                await transaction.CommitAsync(
                    cancellationToken);

                _logger.LogInformation(
                    "Thanh toán VNPay {TransactionId} thành công. " +
                    "Đã áp dụng gói {PackageId} cho user {UserId}.",
                    payment.TransactionId,
                    payment.PackageId,
                    payment.UserId);

                return CreateIpnResult(
                    "00",
                    "Confirm success");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                _logger.LogWarning(
                    ex,
                    "Xung đột khi xử lý IPN của Payment {PaymentId}.",
                    payment?.PaymentId);

                return CreateIpnResult(
                    "02",
                    "Order already confirmed");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                _logger.LogError(
                    ex,
                    "Lỗi khi xử lý IPN VNPay cho Payment {PaymentId}.",
                    payment?.PaymentId);

                return CreateIpnResult(
                    "99",
                    "Unknown error");
            }
        }

        public async Task<VnPayReturnResultDto>
            ProcessVnPayReturnAsync(
                IReadOnlyDictionary<string, string> callbackValues)
        {
            var isSignatureValid =
                _paymentProvider.ValidateCallbackSignature(
                    callbackValues);

            if (!isSignatureValid)
            {
                return new VnPayReturnResultDto
                {
                    IsSignatureValid = false,
                    IsProviderSuccessful = false,
                    Message =
                        "Chữ ký phản hồi từ VNPay không hợp lệ."
                };
            }

            var callback =
                _paymentProvider.ParseCallback(callbackValues);

            var payment = await _paymentRepository
                .GetByTransactionIdAsync(
                    callback.TransactionId);

            if (payment == null)
            {
                return new VnPayReturnResultDto
                {
                    IsSignatureValid = true,
                    IsProviderSuccessful =
                        callback.IsSuccessful,

                    TransactionId =
                        callback.TransactionId,

                    Amount =
                        callback.RawAmount / 100m,

                    ResponseCode =
                        callback.ResponseCode,

                    TransactionNo =
                        callback.TransactionNo,

                    BankCode =
                        callback.BankCode,

                    Message =
                        "Không tìm thấy giao dịch trong hệ thống."
                };
            }

            if (payment.Status == PaymentStatuses.Pending)
            {
                await ProcessVnPayIpnAsync(callbackValues);

                payment = await _paymentRepository
                    .GetByTransactionIdAsync(
                        callback.TransactionId)
                    ?? payment;
            }

            return new VnPayReturnResultDto
            {
                IsSignatureValid = true,

                IsProviderSuccessful =
                    callback.IsSuccessful,

                TransactionId =
                    callback.TransactionId,

                Amount =
                    callback.RawAmount / 100m,

                PaymentStatus =
                    payment.Status,

                ResponseCode =
                    callback.ResponseCode,

                TransactionNo =
                    callback.TransactionNo,

                BankCode =
                    callback.BankCode,

                Message = payment.Status ==
                          PaymentStatuses.Success
                    ? "Thanh toán thành công và gói dịch vụ đã được kích hoạt."
                    : callback.IsSuccessful
                        ? "VNPay đã ghi nhận giao dịch. Hệ thống đang chờ IPN xác nhận."
                        : "Giao dịch không thành công hoặc đã bị hủy."
            };
        }

        public Task<List<Payment>> GetUserPaymentsAsync(
            int userId)
        {
            return _paymentRepository.GetByUserAsync(userId);
        }

        public Task<List<Payment>> GetAllPaymentsAsync()
        {
            return _paymentRepository.GetAllAsync();
        }

        private static void ApplyVnPayCallbackData(
            Payment payment,
            VnPayCallbackData callback)
        {
            payment.VnPayResponseCode =
                callback.ResponseCode;

            payment.VnPayTransactionStatus =
                callback.TransactionStatus;

            payment.VnPayBankCode =
                callback.BankCode;

            payment.VnPayCardType =
                callback.CardType;

            payment.VnPayPayDate =
                callback.PayDate;

            if (callback.IsSuccessful &&
                !string.IsNullOrWhiteSpace(callback.TransactionNo) &&
                callback.TransactionNo != "0")
            {
                payment.ProviderReference =
                    callback.TransactionNo;
            }
        }

        private static string? NormalizeBankCode(
            string? bankCode)
        {
            if (string.IsNullOrWhiteSpace(bankCode))
            {
                return null;
            }

            var normalized =
                bankCode.Trim().ToUpperInvariant();

            return normalized switch
            {
                "VNPAYQR" => "VNPAYQR",
                "VNBANK" => "VNBANK",

                _ => throw new InvalidOperationException(
                    "Phương thức thanh toán VNPay không hợp lệ.")
            };
        }

        private static VnPayIpnResultDto CreateIpnResult(
            string responseCode,
            string message)
        {
            return new VnPayIpnResultDto
            {
                RspCode = responseCode,
                Message = message
            };
        }

        private static string CreateTransactionId()
        {
            return $"PAY{DateTime.Now:yyyyMMddHHmmss}" +
                   $"{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        }
    }
}
