using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IChapterService
    {
        Task<List<Chapter>> GetAllAsync();

        Task<List<Chapter>> GetBySubjectIdAsync(int subjectId);

        Task<Chapter?> GetByIdAsync(int id);

        Task CreateAsync(
            int subjectId,
            string chapterName,
            int orderIndex);

        Task DeleteAsync(int id);
    }
}
