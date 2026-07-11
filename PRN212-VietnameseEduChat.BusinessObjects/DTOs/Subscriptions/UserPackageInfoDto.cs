using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Subscriptions
{
    public class UserPackageInfoDto
    {
        public Package Package { get; set; } = null!;

        public UserSubscription? Subscription { get; set; }

        public int QuestionsUsedToday { get; set; }

        public int DocumentsOwned { get; set; }

        public bool IsDefaultFreeTier { get; set; }
    }
}
