using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research
{
    public class ResearchQuestionImportResultDto
    {
        public int TotalRows { get; set; }

        public int SuccessCount { get; set; }

        public int FailedCount { get; set; }

        public List<string> Errors { get; set; } = new();
    }
}
