using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Options
{
    public sealed class VnPaySettings
    {
        public string PaymentUrl { get; set; }
            = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

        public string TmnCode { get; set; } = string.Empty;

        public string HashSecret { get; set; } = string.Empty;

        /// <summary>
        /// Domain HTTPS public, không có dấu / cuối.
        /// </summary>
        public string PublicBaseUrl { get; set; }
            = string.Empty;

        public string Version { get; set; } = "2.1.0";

        public string Command { get; set; } = "pay";

        public string OrderType { get; set; } = "other";

        public string Locale { get; set; } = "vn";

        public int ExpirationMinutes { get; set; } = 15;
    }
}
