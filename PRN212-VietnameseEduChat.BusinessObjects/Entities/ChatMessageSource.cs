using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class ChatMessageSource
    {
        public int ChatMessageSourceId { get; set; }

        public int ChatMessageId { get; set; }

        public int DocumentChunkId { get; set; }

        public double SimilarityScore { get; set; }

        public string? Excerpt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ChatMessage? ChatMessage { get; set; }

        public DocumentChunk? DocumentChunk { get; set; }
    }
}
