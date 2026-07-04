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
    public class ChapterRepository : IChapterRepository
    {
        private readonly ApplicationDbContext _context;

        public ChapterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Chapter>> GetAllAsync()
        {
            return await _context.Chapters
                .Include(x => x.Subject)
                .OrderBy(x => x.Subject!.SubjectName)
                .ThenBy(x => x.OrderIndex)
                .ToListAsync();
        }

        public async Task<List<Chapter>> GetBySubjectIdAsync(int subjectId)
        {
            return await _context.Chapters
                .Where(x => x.SubjectId == subjectId)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync();
        }

        public async Task<Chapter?> GetByIdAsync(int id)
        {
            return await _context.Chapters
                .Include(x => x.Subject)
                .FirstOrDefaultAsync(x => x.ChapterId == id);
        }

        public async Task AddAsync(Chapter chapter)
        {
            _context.Chapters.Add(chapter);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Chapter chapter)
        {
            _context.Chapters.Update(chapter);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Chapter chapter)
        {
            _context.Chapters.Remove(chapter);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasDocumentsAsync(int chapterId)
        {
            return await _context.Documents
                .AnyAsync(x => x.ChapterId == chapterId);
        }
    }
}
