using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface ISubjectLecturerService
    {
        Task<List<User>> GetLecturersAsync();

        Task<List<Subject>> GetAssignedSubjectsAsync(int lecturerId);

        Task<bool> IsLecturerAssignedAsync(
            int subjectId,
            int lecturerId);

        Task AssignAsync(
            int subjectId,
            int lecturerId,
            int assignedBy);

        Task UnassignAsync(
            int subjectId,
            int lecturerId);
    }
}
