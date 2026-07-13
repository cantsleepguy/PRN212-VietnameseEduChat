using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Payments;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>
        /// Tạo Payment Pending và trả về trang chọn phương thức VNPay.
        /// </summary>
        Task<string> InitiatePaymentAsync(
            int userId,
            int packageId);

        /// <summary>
        /// Tạo URL thanh toán VNPay Sandbox.
        /// bankCode:
        /// - VNPAYQR
        /// - VNBANK
        /// - null để người dùng tự chọn trên VNPay
        /// </summary>
        Task<string> CreateVnPayUrlAsync(
            string transactionId,
            int userId,
            string? bankCode,
            string clientIpAddress,
            CancellationToken cancellationToken = default);

        Task<Payment?> GetByTransactionIdAsync(
            string transactionId);

        Task CancelPendingPaymentAsync(
            string transactionId,
            int userId);

        Task<VnPayIpnResultDto> ProcessVnPayIpnAsync(
            IReadOnlyDictionary<string, string> callbackValues,
            CancellationToken cancellationToken = default);

        Task<VnPayReturnResultDto> ProcessVnPayReturnAsync(
            IReadOnlyDictionary<string, string> callbackValues);

        Task<List<Payment>> GetUserPaymentsAsync(
            int userId);

        Task<List<Payment>> GetAllPaymentsAsync();
    }
}
