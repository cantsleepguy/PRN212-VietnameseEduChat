using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface ISubjectService
    {
        Task<List<Subject>> GetAllAsync();

        Task<Subject?> GetByIdAsync(int id);

        Task CreateAsync(string subjectName, string? description);

        Task DeleteAsync(int id);
    }
}
