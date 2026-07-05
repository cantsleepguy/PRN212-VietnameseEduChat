using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research
{
    public class ResearchResultDto
    {
        public int ResearchResultId { get; set; }

        public int ResearchQuestionId { get; set; }

        public string Question { get; set; } = string.Empty;

        public string GroundTruthAnswer { get; set; } = string.Empty;

        public string GeneratedAnswer { get; set; } = string.Empty;

        public string RetrievedSourcesJson { get; set; } = string.Empty;

        public double AnswerSimilarityScore { get; set; }

        public double ContextRelevanceScore { get; set; }

        public double GroundednessScore { get; set; }

        public double KeywordHitScore { get; set; }

        public double OverallScore { get; set; }

        public long LatencyMs { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
