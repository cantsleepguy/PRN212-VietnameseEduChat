using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    /// <summary>
    /// Provider thanh toán giả lập. Chuyển hướng người dùng đến trang
    /// xác nhận thanh toán nội bộ thay vì cổng thanh toán thật.
    /// Có thể thay bằng VNPayPaymentProvider mà không đổi luồng xử lý.
    /// </summary>
    public class MockPaymentProvider : IPaymentProvider
    {
        public string ProviderName => "Mock";

        public Task<PaymentInitResult> InitiatePaymentAsync(Payment payment)
        {
            var transactionId = $"MOCK-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"
                .Substring(0, 40);

            var result = new PaymentInitResult
            {
                TransactionId = transactionId,
                RedirectUrl = $"/Packages/MockCheckout?transactionId={transactionId}"
            };

            return Task.FromResult(result);
        }
    }
}
