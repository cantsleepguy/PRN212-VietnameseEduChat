using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class DocumentChunk
    {
        public int DocumentChunkId { get; set; }

        public int DocumentId { get; set; }

        public Document? Document { get; set; }

        public int ChunkIndex { get; set; }

        public int? PageNumber { get; set; }

        public string Content { get; set; } = string.Empty;

        public string EmbeddingJson { get; set; } = string.Empty;

        public string EmbeddingModel { get; set; } = string.Empty;

        public int EmbeddingDimensions { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
