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
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;

        public SubjectService(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<List<Subject>> GetAllAsync()
        {
            return await _subjectRepository.GetAllAsync();
        }

        public async Task<Subject?> GetByIdAsync(int id)
        {
            return await _subjectRepository.GetByIdAsync(id);
        }

        public async Task CreateAsync(
            string subjectName,
            string? description)
        {
            if (string.IsNullOrWhiteSpace(subjectName))
            {
                throw new InvalidOperationException(
                    "Tên môn học không được để trống.");
            }

            var subject = new Subject
            {
                SubjectName = subjectName.Trim(),
                Description = description?.Trim()
            };

            await _subjectRepository.AddAsync(subject);
        }

        public async Task DeleteAsync(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);

            if (subject == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy môn học.");
            }

            if (subject.Chapters.Count > 0)
            {
                throw new InvalidOperationException(
                    "Không thể xóa môn học đang có chương. Hãy xóa các chương trước.");
            }

            var hasDocuments = await _subjectRepository
                .HasDocumentsAsync(id);

            if (hasDocuments)
            {
                throw new InvalidOperationException(
                    "Không thể xóa môn học đang có tài liệu.");
            }

            await _subjectRepository.DeleteAsync(subject);
        }
    }
}
