using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Payments;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public sealed class PaymentInitResult
    {
        public string RedirectUrl { get; set; }
            = string.Empty;

        public string OrderInfo { get; set; }
            = string.Empty;

        public string? RequestedBankCode { get; set; }
    }

    public interface IPaymentProvider
    {
        string ProviderName { get; }

        Task<PaymentInitResult> CreatePaymentUrlAsync(
            Payment payment,
            string clientIpAddress,
            string? bankCode,
            CancellationToken cancellationToken = default);

        bool ValidateCallbackSignature(
            IReadOnlyDictionary<string, string> values);

        VnPayCallbackData ParseCallback(
            IReadOnlyDictionary<string, string> values);
    }
}
