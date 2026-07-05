using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research
{
    public class ResearchChunkingStrategyOptionDto
    {
        public string Key { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int ChunkSize { get; set; }

        public int ChunkOverlap { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
