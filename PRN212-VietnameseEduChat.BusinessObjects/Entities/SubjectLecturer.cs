using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.Entities
{
    public class SubjectLecturer
    {
        public int SubjectLecturerId { get; set; }

        public int SubjectId { get; set; }

        public Subject? Subject { get; set; }

        public int LecturerId { get; set; }

        public User? Lecturer { get; set; }

        public DateTime AssignedAt { get; set; }

        public int AssignedBy { get; set; }

        public User? AssignedByUser { get; set; }
    }
}
