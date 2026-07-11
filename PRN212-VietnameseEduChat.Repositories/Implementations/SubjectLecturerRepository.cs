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
    public class SubjectLecturerRepository : ISubjectLecturerRepository
    {
        private readonly ApplicationDbContext _context;

        public SubjectLecturerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubjectLecturer>> GetBySubjectIdAsync(
            int subjectId)
        {
            return await _context.SubjectLecturers
                .Include(x => x.Lecturer)
                .Where(x => x.SubjectId == subjectId)
                .OrderBy(x => x.Lecturer!.FullName)
                .ToListAsync();
        }

        public async Task<List<Subject>> GetAssignedSubjectsAsync(
            int lecturerId)
        {
            return await _context.SubjectLecturers
                .Where(x => x.LecturerId == lecturerId)
                .Include(x => x.Subject)
                    .ThenInclude(x => x!.Chapters)
                .Include(x => x.Subject)
                    .ThenInclude(x => x!.Documents)
                .Select(x => x.Subject!)
                .OrderBy(x => x.SubjectName)
                .ToListAsync();
        }

        public async Task<SubjectLecturer?> GetAsync(
            int subjectId,
            int lecturerId)
        {
            return await _context.SubjectLecturers
                .Include(x => x.Subject)
                .Include(x => x.Lecturer)
                .FirstOrDefaultAsync(x =>
                    x.SubjectId == subjectId &&
                    x.LecturerId == lecturerId);
        }

        public async Task<bool> ExistsAsync(
            int subjectId,
            int lecturerId)
        {
            return await _context.SubjectLecturers
                .AnyAsync(x =>
                    x.SubjectId == subjectId &&
                    x.LecturerId == lecturerId);
        }

        public async Task AddAsync(SubjectLecturer assignment)
        {
            _context.SubjectLecturers.Add(assignment);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(SubjectLecturer assignment)
        {
            _context.SubjectLecturers.Remove(assignment);

            await _context.SaveChangesAsync();
        }
    }
}
