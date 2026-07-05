using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class ResearchDocumentChunk
    {
        public int ResearchDocumentChunkId { get; set; }

        public int DocumentId { get; set; }

        public string ChunkingStrategyKey { get; set; } = string.Empty;

        public string ChunkingStrategyName { get; set; } = string.Empty;

        public int ChunkSize { get; set; }

        public int ChunkOverlap { get; set; }

        public int ChunkIndex { get; set; }

        public string Content { get; set; } = string.Empty;

        public string EmbeddingProvider { get; set; } = "OpenAI";

        public string EmbeddingModelName { get; set; } = string.Empty;

        public int EmbeddingDimensions { get; set; }

        public string EmbeddingJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Document? Document { get; set; }
    }
}
