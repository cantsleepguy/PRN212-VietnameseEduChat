using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Chats
{
    public class ChatAskRequestDto
    {
        public int? ChatSessionId { get; set; }

        public int? SubjectId { get; set; }

        public string Question { get; set; } = string.Empty;
    }
}
