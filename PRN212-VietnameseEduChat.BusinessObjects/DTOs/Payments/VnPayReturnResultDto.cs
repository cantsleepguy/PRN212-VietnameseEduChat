using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Payments
{
    public sealed class VnPayReturnResultDto
    {
        public bool IsSignatureValid { get; set; }

        public bool IsProviderSuccessful { get; set; }

        public string TransactionId { get; set; }
            = string.Empty;

        public decimal Amount { get; set; }

        public string PaymentStatus { get; set; }
            = string.Empty;

        public string ResponseCode { get; set; }
            = string.Empty;

        public string TransactionNo { get; set; }
            = string.Empty;

        public string BankCode { get; set; }
            = string.Empty;

        public string Message { get; set; }
            = string.Empty;
    }
}
