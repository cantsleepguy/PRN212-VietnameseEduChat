using Microsoft.AspNetCore.Http;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IResearchQuestionService
    {
        Task<List<ResearchQuestionDto>> GetAllAsync();

        Task<ResearchQuestionDto?> GetByIdAsync(int id);

        Task<ResearchQuestionFormOptionsDto> GetFormOptionsAsync();

        Task CreateAsync(ResearchQuestionInputDto input);

        Task UpdateAsync(int id, ResearchQuestionInputDto input);

        Task DeleteAsync(int id);

        Task<int> CountAsync();

        Task<ResearchQuestionImportResultDto> ImportCsvAsync(IFormFile file);
    }
}
