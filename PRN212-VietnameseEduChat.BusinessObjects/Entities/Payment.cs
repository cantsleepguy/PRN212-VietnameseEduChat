using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PRN212_VietnameseEduChat.BusinessObjects.Constants;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public int PackageId { get; set; }

        public Package? Package { get; set; }

        /// <summary>
        /// Purchase | Renewal | Upgrade
        /// </summary>
        public string PaymentType { get; set; }
            = PaymentTypes.Purchase;

        /// <summary>
        /// Subscription hiện tại được dùng để tính nâng cấp.
        /// Chỉ có giá trị khi PaymentType là Upgrade.
        /// </summary>
        public int? SourceSubscriptionId { get; set; }

        public UserSubscription? SourceSubscription { get; set; }

        /// <summary>
        /// Giá toàn bộ gói tại thời điểm tạo đơn.
        /// </summary>
        public decimal PackagePriceSnapshot { get; set; }

        public string PackageCodeSnapshot { get; set; }
            = string.Empty;

        public string PackageNameSnapshot { get; set; }
            = string.Empty;

        public int PackageDurationDaysSnapshot { get; set; }

        /// <summary>
        /// Giá trị gói mới áp dụng cho thời gian được mua.
        /// Với upgrade, đây là giá trị gói mới trong thời gian còn lại.
        /// </summary>
        public decimal GrossAmount { get; set; }

        /// <summary>
        /// Giá trị chưa sử dụng của gói cũ được khấu trừ.
        /// </summary>
        public decimal CreditAmount { get; set; }

        /// <summary>
        /// Số tiền người dùng thực tế phải thanh toán.
        /// </summary>
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "VND";

        /// <summary>
        /// Pending | Success | Cancelled | Expired |
        /// Failed | AmountMismatch
        /// </summary>
        public string Status { get; set; }
            = PaymentStatuses.Pending;

        /// <summary>
        /// Mã nội bộ được hiển thị trong lịch sử.
        /// </summary>
        public string TransactionId { get; set; }
            = string.Empty;

        /// <summary>
        /// Mã đơn hàng gửi tới payOS.
        /// </summary>
        public long? OrderCode { get; set; }

        public string Provider { get; set; }
            = PaymentProviders.VnPay;

        public string? ProviderPaymentLinkId { get; set; }

        /// <summary>
        /// Mã vnp_TransactionNo do VNPay trả về.
        /// </summary>
        public string? ProviderReference { get; set; }

        public string? CheckoutUrl { get; set; }

        /// <summary>
        /// Chuỗi dữ liệu VietQR, không phải URL ảnh.
        /// </summary>
        public string? QrCode { get; set; }

        public string? BankBin { get; set; }

        public string? BankAccountNumber { get; set; }

        public string? BankAccountName { get; set; }

        public string? TransferDescription { get; set; }

        /// <summary>
        /// Khi upgrade, gói mới sẽ giữ ngày hết hạn này.
        /// </summary>
        public DateTime? TargetEndDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime? PaidAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public string? FailureReason { get; set; }

        /// <summary>
        /// Chống hai request/webhook cập nhật cùng Payment.
        /// </summary>
        public byte[] RowVersion { get; set; }
            = Array.Empty<byte>();

        /// <summary>
        /// Ngày gói mới bắt đầu.
        /// Dùng cho ScheduledDowngrade.
        /// </summary>
        public DateTime? TargetStartDate { get; set; }

        public string? VnPayResponseCode { get; set; }

        public string? VnPayTransactionStatus { get; set; }

        public string? VnPayBankCode { get; set; }

        public string? VnPayCardType { get; set; }

        public DateTime? VnPayPayDate { get; set; }
    }
}
