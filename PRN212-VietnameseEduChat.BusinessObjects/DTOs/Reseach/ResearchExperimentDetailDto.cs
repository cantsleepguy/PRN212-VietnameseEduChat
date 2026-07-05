using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research
{
    public class ResearchExperimentDetailDto
    {
        public ResearchExperimentDto Experiment { get; set; } = new();

        public List<ResearchResultDto> Results { get; set; } = new();
    }
}
