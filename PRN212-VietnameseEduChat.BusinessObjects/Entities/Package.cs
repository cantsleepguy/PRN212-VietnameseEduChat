using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class Package
    {
        public int PackageId { get; set; }

        /// <summary>
        /// Free | Premium | Enterprise
        /// </summary>
        public string PackageCode { get; set; } = string.Empty;

        public string PackageName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int DurationDays { get; set; }

        /// <summary>
        /// Số câu hỏi tối đa mỗi ngày. Null = không giới hạn.
        /// </summary>
        public int? DailyQuestionLimit { get; set; }

        /// <summary>
        /// Dung lượng upload tối đa mỗi file (MB).
        /// </summary>
        public int MaxUploadSizeMb { get; set; }

        /// <summary>
        /// Số tài liệu tối đa người dùng được sở hữu. Null = không giới hạn.
        /// </summary>
        public int? MaxDocuments { get; set; }

        public bool AllowAiFeatures { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<UserSubscription> Subscriptions { get; set; }
            = new List<UserSubscription>();

        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();
    }
}
