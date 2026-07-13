using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Payments
{
    public sealed class VnPayCallbackData
    {
        public string TransactionId { get; set; }
            = string.Empty;

        /// <summary>
        /// Giá trị vnp_Amount chưa chia 100.
        /// </summary>
        public long RawAmount { get; set; }

        public string ResponseCode { get; set; }
            = string.Empty;

        public string TransactionStatus { get; set; }
            = string.Empty;

        public string TransactionNo { get; set; }
            = string.Empty;

        public string BankCode { get; set; }
            = string.Empty;

        public string CardType { get; set; }
            = string.Empty;

        public string OrderInfo { get; set; }
            = string.Empty;

        public DateTime? PayDate { get; set; }

        public bool IsSuccessful =>
            ResponseCode == "00" &&
            TransactionStatus == "00";
    }
}
