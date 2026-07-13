using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.Pages.Packages
{
    [Authorize]
    public class CheckoutModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<CheckoutModel> _logger;

        public CheckoutModel(
            IPaymentService paymentService,
            ILogger<CheckoutModel> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        public Payment? Payment { get; set; }

        [BindProperty]
        public string TransactionId { get; set; }
            = string.Empty;

        public async Task<IActionResult> OnGetAsync(
            string? transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                TempData["ErrorMessage"] =
                    "Thiếu mã giao dịch.";

                return RedirectToPage("/Packages/Index");
            }

            var userId = GetCurrentUserId();

            Payment = await _paymentService
                .GetByTransactionIdAsync(transactionId);

            if (Payment == null ||
                Payment.UserId != userId)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy giao dịch.";

                return RedirectToPage("/Packages/Index");
            }

            TransactionId = Payment.TransactionId;

            return Page();
        }

        /// <summary>
        /// Chuyển người dùng sang VNPay Sandbox.
        ///
        /// bankCode:
        /// - VNPAYQR: thanh toán bằng QR
        /// - VNBANK: tài khoản/thẻ ngân hàng nội địa
        /// - null hoặc rỗng: tự chọn phương thức trên VNPay
        /// </summary>
        public async Task<IActionResult> OnPostPayAsync(
            string? bankCode,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(TransactionId))
            {
                TempData["ErrorMessage"] =
                    "Thiếu mã giao dịch.";

                return RedirectToPage("/Packages/Index");
            }

            try
            {
                var clientIpAddress =
                    GetClientIpAddress();

                var redirectUrl =
                    await _paymentService
                        .CreateVnPayUrlAsync(
                            TransactionId,
                            userId,
                            bankCode,
                            clientIpAddress,
                            cancellationToken);

                return Redirect(redirectUrl);
            }
            catch (OperationCanceledException)
            {
                TempData["ErrorMessage"] =
                    "Yêu cầu thanh toán đã bị hủy.";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Không thể chuyển tới VNPay cho giao dịch {TransactionId}.",
                    TransactionId);

                TempData["ErrorMessage"] =
                    ex.Message;
            }

            return RedirectToPage(
                "/Packages/Checkout",
                new
                {
                    transactionId = TransactionId
                });
        }

        public async Task<JsonResult> OnGetStatusAsync(
            string transactionId)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                return new JsonResult(new
                {
                    found = false
                });
            }

            var payment = await _paymentService
                .GetByTransactionIdAsync(transactionId);

            if (payment == null ||
                payment.UserId != userId)
            {
                return new JsonResult(new
                {
                    found = false
                });
            }

            return new JsonResult(new
            {
                found = true,
                status = payment.Status,
                failureReason = payment.FailureReason,
                paidAt = payment.PaidAt
            });
        }

        public async Task<IActionResult> OnPostCancelAsync()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(TransactionId))
            {
                TempData["ErrorMessage"] =
                    "Thiếu mã giao dịch.";

                return RedirectToPage("/Packages/Index");
            }

            try
            {
                await _paymentService
                    .CancelPendingPaymentAsync(
                        TransactionId,
                        userId);

                TempData["SuccessMessage"] =
                    "Đã hủy giao dịch.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;
            }

            return RedirectToPage("/Packages/History");
        }

        private int GetCurrentUserId()
        {
            var value = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(value, out var userId))
            {
                throw new InvalidOperationException(
                    "Không xác định được người dùng hiện tại.");
            }

            return userId;
        }

        private string GetClientIpAddress()
        {
            var ipAddress = HttpContext
                .Connection
                .RemoteIpAddress?
                .ToString();

            return string.IsNullOrWhiteSpace(ipAddress)
                ? "127.0.0.1"
                : ipAddress;
        }
    }
}