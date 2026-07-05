using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class ChatMessage
    {
        public int ChatMessageId { get; set; }

        public int ChatSessionId { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ChatSession? ChatSession { get; set; }

        public ICollection<ChatMessageSource> Sources { get; set; } = new List<ChatMessageSource>();
    }
}
