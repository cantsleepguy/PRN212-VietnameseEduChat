using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Chats
{
    public class ChatStreamEventDto
    {
        /// <summary>
        /// Session | Sources | Token | Done | Error
        /// </summary>
        public string Type { get; set; } = string.Empty;

        public int? ChatSessionId { get; set; }

        public string? SessionTitle { get; set; }

        public string? Token { get; set; }

        public string? Message { get; set; }

        public List<ChatSourceDto>? Sources { get; set; }
    }
}
