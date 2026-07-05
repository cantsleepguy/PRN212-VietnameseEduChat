using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Interfaces
{
    public interface ISubjectLecturerRepository
    {
        Task<List<SubjectLecturer>> GetBySubjectIdAsync(int subjectId);

        Task<List<Subject>> GetAssignedSubjectsAsync(int lecturerId);

        Task<SubjectLecturer?> GetAsync(
            int subjectId,
            int lecturerId);

        Task<bool> ExistsAsync(
            int subjectId,
            int lecturerId);

        Task AddAsync(SubjectLecturer assignment);

        Task DeleteAsync(SubjectLecturer assignment);
    }
}
