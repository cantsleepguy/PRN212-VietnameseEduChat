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
                .Include(x => x.Reviewer)
                .Include(x => x.Subject)
                .Include(x => x.Chapter)
                .OrderByDescending(x => x.UploadedAt)
                .ToListAsync();
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _context.Documents
                .Include(x => x.User)
                .Include(x => x.Reviewer)
                .Include(x => x.Subject)
                .Include(x => x.Chapter)
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

        public async Task DeleteChunksByDocumentAsync(int documentId)
        {
            var sources = await _context.ChatMessageSources
                .Where(s =>
                    s.DocumentChunk != null &&
                    s.DocumentChunk.DocumentId == documentId)
                .ToListAsync();

            if (sources.Count > 0)
            {
                _context.ChatMessageSources.RemoveRange(sources);
            }

            var chunks = await _context.DocumentChunks
                .Where(c => c.DocumentId == documentId)
                .ToListAsync();

            if (chunks.Count > 0)
            {
                _context.DocumentChunks.RemoveRange(chunks);
            }

            if (sources.Count > 0 || chunks.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Document?> GetByIdWithChunksAsync(int id)
        {
            return await _context.Documents
                .Include(x => x.User)
                .Include(x => x.Reviewer)
                .Include(x => x.Subject)
                .Include(x => x.Chapter)
                .Include(x => x.Chunks)
                .FirstOrDefaultAsync(x => x.DocumentId == id);
        }

        public Task<List<Document>> GetPendingProcessingAsync(
            CancellationToken cancellationToken = default)
        {
            return _context.Documents
                .Where(x => x.Status == "Queued" || x.Status == "Processing")
                .OrderBy(x => x.UploadedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
