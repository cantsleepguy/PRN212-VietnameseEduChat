using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Constants
{
    public static class PaymentStatuses
    {
        public const string Pending = "Pending";
        public const string Success = "Success";
        public const string Cancelled = "Cancelled";
        public const string Expired = "Expired";
        public const string Failed = "Failed";
        public const string AmountMismatch = "AmountMismatch";
    }
}
