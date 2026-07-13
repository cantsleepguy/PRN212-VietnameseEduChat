using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class UserSubscription
    {
        public int UserSubscriptionId { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public int PackageId { get; set; }

        public Package? Package { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        /// <summary>
        /// Active | Expired | Cancelled
        /// </summary>
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
