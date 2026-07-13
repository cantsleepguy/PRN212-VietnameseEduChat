using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Payments
{
    public sealed class VnPayIpnResultDto
    {
        public string RspCode { get; set; } = "99";

        public string Message { get; set; }
            = "Unknown error";
    }
}
