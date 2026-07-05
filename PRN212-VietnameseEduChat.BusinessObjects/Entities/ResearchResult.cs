using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class ResearchResult
    {
        public int ResearchResultId { get; set; }

        public int ResearchExperimentId { get; set; }

        public int ResearchQuestionId { get; set; }

        public string GeneratedAnswer { get; set; } = string.Empty;

        public string RetrievedContext { get; set; } = string.Empty;

        public string RetrievedSourcesJson { get; set; } = string.Empty;

        public double AnswerSimilarityScore { get; set; }

        public double ContextRelevanceScore { get; set; }

        public double GroundednessScore { get; set; }

        public double KeywordHitScore { get; set; }

        public double OverallScore { get; set; }

        public long LatencyMs { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ResearchExperiment? ResearchExperiment { get; set; }

        public ResearchQuestion? ResearchQuestion { get; set; }
    }
}
