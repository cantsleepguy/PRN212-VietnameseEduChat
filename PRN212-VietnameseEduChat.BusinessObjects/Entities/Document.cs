using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class Document
    {
        public int DocumentId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }

        public int UploadedBy { get; set; }

        public User? User { get; set; }

        public string Status { get; set; } = "Pending";

        public int TotalChunks { get; set; }

        public string? ErrorMessage { get; set; }

        public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
    }
}
