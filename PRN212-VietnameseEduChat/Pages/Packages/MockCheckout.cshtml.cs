using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Pages.Packages
{
    [Authorize]
    public class MockCheckoutModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public MockCheckoutModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public Payment? Payment { get; set; }

        [BindProperty]
        public string TransactionId { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string? transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                TempData["ErrorMessage"] = "Thiếu mã giao dịch.";
                return RedirectToPage("/Packages/Index");
            }

            var userId = GetCurrentUserId();

            Payment = await _paymentService.GetByTransactionIdAsync(
                transactionId);

            if (Payment == null || Payment.UserId != userId)
            {
                TempData["ErrorMessage"] = "Không tìm thấy giao dịch.";
                return RedirectToPage("/Packages/Index");
            }

            if (Payment.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Giao dịch này đã được xử lý.";
                return RedirectToPage("/Packages/Index");
            }

            TransactionId = transactionId;

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync()
        {
            return await ProcessAsync(true);
        }

        public async Task<IActionResult> OnPostCancelAsync()
        {
            return await ProcessAsync(false);
        }

        private async Task<IActionResult> ProcessAsync(bool isSuccessful)
        {
            var userId = GetCurrentUserId();

            try
            {
                await _paymentService.ConfirmPaymentAsync(
                    TransactionId,
                    userId,
                    isSuccessful);

                if (isSuccessful)
                {
                    TempData["SuccessMessage"] =
                        "Thanh toán thành công! Gói dịch vụ đã được kích hoạt.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Bạn đã hủy giao dịch.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage("/Packages/Index");
        }

        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                throw new InvalidOperationException(
                    "Không xác định được người dùng hiện tại.");
            }

            return userId;
        }
    }
}
