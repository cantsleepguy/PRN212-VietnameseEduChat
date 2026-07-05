using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research
{
    public class ResearchQuestionInputDto
    {
        public int? SubjectId { get; set; }

        public int? ChapterId { get; set; }

        public int? SourceDocumentId { get; set; }

        public string Question { get; set; } = string.Empty;

        public string GroundTruthAnswer { get; set; } = string.Empty;

        public string? ExpectedKeywords { get; set; }

        public string? ExpectedSource { get; set; }
    }
}
