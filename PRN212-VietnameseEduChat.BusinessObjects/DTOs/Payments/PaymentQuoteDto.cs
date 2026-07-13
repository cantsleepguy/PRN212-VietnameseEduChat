using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Payments
{
    public sealed class PaymentQuoteDto
    {
        public Package TargetPackage { get; set; } = null!;

        public UserSubscription? SourceSubscription { get; set; }

        public string PaymentType { get; set; } = string.Empty;

        public decimal GrossAmount { get; set; }

        public decimal CreditAmount { get; set; }

        public decimal Amount { get; set; }

        public DateTime? TargetStartDate { get; set; }

        public DateTime? TargetEndDate { get; set; }
    }
}
