using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class ResearchExperiment
    {
        public int ResearchExperimentId { get; set; }

        public string ExperimentName { get; set; } = string.Empty;

        public string ExperimentType { get; set; } = "RAG";

        public string AnswerModelName { get; set; } = string.Empty;

        public string EmbeddingProvider { get; set; } = "OpenAI";

        public string EmbeddingModelName { get; set; } = string.Empty;

        public int EmbeddingDimensions { get; set; }

        public string ChunkingStrategyKey { get; set; } = "fixed-baseline";

        public string ChunkingStrategyName { get; set; } = string.Empty;

        public int ChunkSize { get; set; }

        public int ChunkOverlap { get; set; }

        public int TopK { get; set; } = 5;

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public string? Notes { get; set; }

        public ICollection<ResearchResult> Results { get; set; }
            = new List<ResearchResult>();
    }
}
