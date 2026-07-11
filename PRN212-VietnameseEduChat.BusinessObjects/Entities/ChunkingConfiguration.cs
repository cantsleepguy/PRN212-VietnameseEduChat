using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class ChunkingConfiguration
    {
        public int ChunkingConfigurationId { get; set; }

        /// <summary>
        /// Paragraph | Character | FixedSize
        /// </summary>
        public string StrategyKey { get; set; } = string.Empty;

        public string StrategyName { get; set; } = string.Empty;

        public int ChunkSize { get; set; }

        public int ChunkOverlap { get; set; }

        /// <summary>
        /// Characters | Tokens (chỉ dùng cho FixedSize)
        /// </summary>
        public string FixedSizeUnit { get; set; } = "Characters";

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public User? UpdatedByUser { get; set; }
    }
}
