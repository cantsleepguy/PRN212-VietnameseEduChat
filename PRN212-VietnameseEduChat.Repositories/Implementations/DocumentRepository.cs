using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Implementations
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Document document)
        {
            _context.Documents.Add(document);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Document document)
        {
            _context.Documents.Remove(document);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Document>> GetAllAsync()
        {
            return await _context.Documents
                .Include(x => x.User)
                .OrderByDescending(x => x.UploadedAt)
                .ToListAsync();
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _context.Documents
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.DocumentId == id);
        }

        public async Task UpdateAsync(Document document)
        {
            _context.Documents.Update(document);

            await _context.SaveChangesAsync();
        }

        public async Task AddChunksAsync(List<DocumentChunk> chunks)
        {
            if (chunks.Count == 0)
                return;

            _context.DocumentChunks.AddRange(chunks);

            await _context.SaveChangesAsync();
        }
    }
}
