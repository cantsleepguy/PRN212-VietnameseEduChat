using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research
{
    public class ResearchScoredChunkDto
    {
        public int ResearchDocumentChunkId { get; set; }

        public int DocumentId { get; set; }

        public int ChunkIndex { get; set; }

        public string Content { get; set; } = string.Empty;

        public string? DocumentName { get; set; }

        public double SimilarityScore { get; set; }
    }
}
