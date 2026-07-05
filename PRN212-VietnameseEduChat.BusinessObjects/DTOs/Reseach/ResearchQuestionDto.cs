using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research
{
    public class ResearchQuestionDto
    {
        public int ResearchQuestionId { get; set; }

        public int? SubjectId { get; set; }

        public string? SubjectName { get; set; }

        public int? ChapterId { get; set; }

        public string? ChapterName { get; set; }

        public int? SourceDocumentId { get; set; }

        public string? SourceDocumentName { get; set; }

        public string Question { get; set; } = string.Empty;

        public string GroundTruthAnswer { get; set; } = string.Empty;

        public string? ExpectedKeywords { get; set; }

        public string? ExpectedSource { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
