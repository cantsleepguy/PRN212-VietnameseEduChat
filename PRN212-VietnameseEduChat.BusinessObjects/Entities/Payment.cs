using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public int PackageId { get; set; }

        public Package? Package { get; set; }

        public decimal Amount { get; set; }

        /// <summary>
        /// Pending | Success | Failed
        /// </summary>
        public string Status { get; set; } = "Pending";

        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Mock | VNPay
        /// </summary>
        public string Provider { get; set; } = "Mock";

        public DateTime CreatedAt { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}
