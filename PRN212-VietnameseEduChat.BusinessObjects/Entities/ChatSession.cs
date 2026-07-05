using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class ChatSession
    {
        public int ChatSessionId { get; set; }

        public int UserId { get; set; }

        public int? SubjectId { get; set; }

        public string Title { get; set; } = "Cuộc trò chuyện mới";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public User? User { get; set; }

        public Subject? Subject { get; set; }

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
