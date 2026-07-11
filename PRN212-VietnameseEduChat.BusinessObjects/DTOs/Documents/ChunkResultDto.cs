using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Documents
{
    public class ChunkResultDto
    {
        public string Content { get; set; } = string.Empty;

        public int? PageNumber { get; set; }
    }
}
