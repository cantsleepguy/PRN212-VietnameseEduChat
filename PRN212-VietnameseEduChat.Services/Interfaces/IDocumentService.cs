using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<List<Document>> GetAllAsync();

        Task<Document?> GetByIdAsync(int id);

        Task UploadAsync(IFormFile file, int userId);

        Task DeleteAsync(int id);
    }
}
