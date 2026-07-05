using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research
{
    public class ResearchExperimentCreateDto
    {
        public string ExperimentName { get; set; } = string.Empty;

        public string ExperimentType { get; set; } = "RAG";

        public string AnswerModelName { get; set; } = "gpt-4o-mini";

        public string EmbeddingProvider { get; set; } = "OpenAI";

        public string EmbeddingModelName { get; set; } = "text-embedding-3-small";

        public int EmbeddingDimensions { get; set; } = 1536;

        public string ChunkingStrategyKey { get; set; } = "fixed-baseline";

        public int TopK { get; set; } = 5;

        public string? Notes { get; set; }
    }
}
