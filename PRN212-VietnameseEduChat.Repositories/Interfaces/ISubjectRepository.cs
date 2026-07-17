using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Interfaces
{
    public interface ISubjectRepository
    {
        Task<List<Subject>> GetAllAsync();

        Task<List<Subject>> GetVisibleAsync();

        Task<Subject?> GetByIdAsync(int id);

        Task AddAsync(Subject subject);

        Task UpdateAsync(Subject subject);

        Task DeleteAsync(Subject subject);

        Task<bool> HasDocumentsAsync(int subjectId);
    }
}
