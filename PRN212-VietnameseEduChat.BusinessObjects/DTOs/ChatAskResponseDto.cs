using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Chats
{
    public class ChatAskResponseDto
    {
        public int ChatSessionId { get; set; }

        public string Answer { get; set; } = string.Empty;

        public List<ChatSourceDto> Sources { get; set; } = new();
    }
}
