using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research
{
    public class ResearchQuestionFormOptionsDto
    {
        public List<ResearchSelectOptionDto> Subjects { get; set; } = new();

        public List<ResearchSelectOptionDto> Chapters { get; set; } = new();

        public List<ResearchSelectOptionDto> Documents { get; set; } = new();
    }
}