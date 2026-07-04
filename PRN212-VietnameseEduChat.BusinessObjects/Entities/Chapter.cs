using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class Chapter
    {
        public int ChapterId { get; set; }

        public int SubjectId { get; set; }

        public Subject? Subject { get; set; }

        public string ChapterName { get; set; } = string.Empty;

        public int OrderIndex { get; set; }

        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
