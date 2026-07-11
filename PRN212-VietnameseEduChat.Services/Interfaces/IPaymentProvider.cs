using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public class PaymentInitResult
    {
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// URL để chuyển hướng người dùng đến trang thanh toán.
        /// Với Mock provider: trang xác nhận thanh toán nội bộ.
        /// </summary>
        public string RedirectUrl { get; set; } = string.Empty;
    }

    public interface IPaymentProvider
    {
        string ProviderName { get; }

        Task<PaymentInitResult> InitiatePaymentAsync(Payment payment);
    }
}
