using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class Subject
    {
        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

        public ICollection<Document> Documents { get; set; } = new List<Document>();

        public ICollection<SubjectLecturer> SubjectLecturers { get; set; } = new List<SubjectLecturer>();
    }
}
