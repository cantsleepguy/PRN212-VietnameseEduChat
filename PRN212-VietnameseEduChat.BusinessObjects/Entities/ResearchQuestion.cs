using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class ResearchQuestion
    {
        public int ResearchQuestionId { get; set; }

        public int? SubjectId { get; set; }

        public int? ChapterId { get; set; }

        public int? SourceDocumentId { get; set; }

        public string Question { get; set; } = string.Empty;

        public string GroundTruthAnswer { get; set; } = string.Empty;

        public string? ExpectedKeywords { get; set; }

        public string? ExpectedSource { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Subject? Subject { get; set; }

        public Chapter? Chapter { get; set; }

        public Document? SourceDocument { get; set; }

        public ICollection<ResearchResult> Results { get; set; }
            = new List<ResearchResult>();
    }
}
