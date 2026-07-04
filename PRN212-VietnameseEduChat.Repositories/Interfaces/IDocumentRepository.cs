using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        Task<List<Document>> GetAllAsync();

        Task<Document?> GetByIdAsync(int id);

        Task<Document?> GetByIdWithChunksAsync(int id);

        Task AddAsync(Document document);

        Task UpdateAsync(Document document);

        Task DeleteAsync(Document document);

        Task AddChunksAsync(List<DocumentChunk> chunks);
    }
}
