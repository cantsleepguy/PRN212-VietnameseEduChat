using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class ChapterService : IChapterService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ISubjectRepository _subjectRepository;

        public ChapterService(
            IChapterRepository chapterRepository,
            ISubjectRepository subjectRepository)
        {
            _chapterRepository = chapterRepository;
            _subjectRepository = subjectRepository;
        }

        public async Task<List<Chapter>> GetAllAsync()
        {
            return await _chapterRepository.GetAllAsync();
        }

        public async Task<List<Chapter>> GetBySubjectIdAsync(int subjectId)
        {
            return await _chapterRepository.GetBySubjectIdAsync(subjectId);
        }

        public async Task<Chapter?> GetByIdAsync(int id)
        {
            return await _chapterRepository.GetByIdAsync(id);
        }

        public async Task CreateAsync(
            int subjectId,
            string chapterName,
            int orderIndex)
        {
            if (string.IsNullOrWhiteSpace(chapterName))
            {
                throw new InvalidOperationException(
                    "Tên chương không được để trống.");
            }

            var subject = await _subjectRepository.GetByIdAsync(subjectId);

            if (subject == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy môn học.");
            }

            var chapter = new Chapter
            {
                SubjectId = subjectId,
                ChapterName = chapterName.Trim(),
                OrderIndex = orderIndex <= 0 ? 1 : orderIndex
            };

            await _chapterRepository.AddAsync(chapter);
        }

        public async Task DeleteAsync(int id)
        {
            var chapter = await _chapterRepository.GetByIdAsync(id);

            if (chapter == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy chương.");
            }

            var hasDocuments = await _chapterRepository
                .HasDocumentsAsync(id);

            if (hasDocuments)
            {
                throw new InvalidOperationException(
                    "Không thể xóa chương đang có tài liệu.");
            }

            await _chapterRepository.DeleteAsync(chapter);
        }
    }
}
