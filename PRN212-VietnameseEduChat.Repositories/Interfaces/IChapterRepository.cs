using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Interfaces
{
    public interface IChapterRepository
    {
        Task<List<Chapter>> GetAllAsync();

        Task<List<Chapter>> GetBySubjectIdAsync(int subjectId);

        Task<Chapter?> GetByIdAsync(int id);

        Task AddAsync(Chapter chapter);

        Task UpdateAsync(Chapter chapter);

        Task DeleteAsync(Chapter chapter);

        Task<bool> HasDocumentsAsync(int chapterId);
    }
}
