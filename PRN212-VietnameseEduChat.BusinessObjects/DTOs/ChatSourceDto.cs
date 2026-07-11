using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Chats
{
    public class ChatSourceDto
    {
        public int DocumentChunkId { get; set; }

        public string? DocumentName { get; set; }

        public int ChunkIndex { get; set; }

        public int? PageNumber { get; set; }

        public int DocumentId { get; set; }

        public string? Excerpt { get; set; }

        public double SimilarityScore { get; set; }
    }
}
