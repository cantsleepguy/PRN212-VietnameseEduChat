using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research
{
    public class ResearchExperimentDto
    {
        public int ResearchExperimentId { get; set; }

        public string ExperimentName { get; set; } = string.Empty;

        public string ExperimentType { get; set; } = string.Empty;

        public string AnswerModelName { get; set; } = string.Empty;

        public string EmbeddingModelName { get; set; } = string.Empty;

        public int EmbeddingDimensions { get; set; }

        public string EmbeddingProvider { get; set; } = string.Empty;

        public string ChunkingStrategyKey { get; set; } = string.Empty;

        public string ChunkingStrategyName { get; set; } = string.Empty;

        public int ChunkSize { get; set; }

        public int ChunkOverlap { get; set; }

        public int TopK { get; set; }

        public string Status { get; set; } = string.Empty;

        public int TotalQuestions { get; set; }

        public int CompletedResults { get; set; }

        public double AverageAnswerSimilarity { get; set; }

        public double AverageContextRelevance { get; set; }

        public double AverageGroundedness { get; set; }

        public double AverageKeywordHit { get; set; }

        public double AverageOverallScore { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }
    }
}
