using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repository;
        private readonly IDocumentFileValidator _fileValidator;
        private readonly IDocumentStorage _storage;
        private readonly IDocumentAccessPolicy _accessPolicy;
        private readonly IDocumentProcessingQueue _processingQueue;
        private readonly ISubjectService _subjectService;
        private readonly IChapterService _chapterService;
        private readonly ISubjectLecturerService _subjectLecturerService;
        private readonly ISubscriptionService _subscriptionService;

        public DocumentService(
            IDocumentRepository repository,
            IDocumentFileValidator fileValidator,
            IDocumentStorage storage,
            IDocumentAccessPolicy accessPolicy,
            IDocumentProcessingQueue processingQueue,
            ISubjectService subjectService,
            IChapterService chapterService,
            ISubjectLecturerService subjectLecturerService,
            ISubscriptionService subscriptionService)
        {
            _repository = repository;
            _fileValidator = fileValidator;
            _storage = storage;
            _accessPolicy = accessPolicy;
            _processingQueue = processingQueue;
            _subjectService = subjectService;
            _chapterService = chapterService;
            _subjectLecturerService = subjectLecturerService;
            _subscriptionService = subscriptionService;
        }

        public async Task UploadAsync(
            IFormFile file,
            int userId,
            int subjectId,
            int chapterId)
        {
            var validatedFile = await _fileValidator.ValidateAsync(file);

            await _subscriptionService.EnsureCanUploadDocumentAsync(
                userId,
                file.Length);

            await ValidateSubjectAndChapterAsync(
                userId,
                subjectId,
                chapterId);

            await using var uploadStream = file.OpenReadStream();
            var stored = await _storage.SaveAsync(
                uploadStream,
                validatedFile.Extension);
            var document = new Document
            {
                Title = Path.GetFileNameWithoutExtension(validatedFile.OriginalFileName),
                OriginalFileName = validatedFile.OriginalFileName,
                StoredFileName = stored.StoredFileName,
                ContentType = validatedFile.ContentType,
                FileSize = file.Length,
                FilePath = stored.StoredFileName,
                UploadedAt = DateTime.Now,
                UploadedBy = userId,
                SubjectId = subjectId,
                ChapterId = chapterId,
                Status = "Queued",
                TotalChunks = 0
            };

            try
            {
                await _repository.AddAsync(document);
            }
            catch
            {
                await _storage.DeleteIfExistsAsync(stored.StoredFileName);
                throw;
            }

            await _processingQueue.EnqueueAsync(document.DocumentId);
        }

        public async Task ReindexAsync(int documentId)
        {
            var document = await _repository.GetByIdAsync(documentId);

            if (document == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy tài liệu.");
            }

            var physicalPath = _storage.GetPhysicalPath(document.StoredFileName);

            if (!File.Exists(physicalPath))
            {
                throw new InvalidOperationException(
                    "Không tìm thấy file gốc của tài liệu để re-index.");
            }

            document.Status = "Queued";
            document.ErrorMessage = null;
            await _repository.UpdateAsync(document);
            await _processingQueue.EnqueueAsync(document.DocumentId);
        }

        public async Task ApproveAsync(int documentId, int reviewerId)
        {
            var document = await _repository.GetByIdAsync(documentId);

            if (document == null)
            {
                throw new InvalidOperationException("Không tìm thấy tài liệu.");
            }

            if (document.Status != "PendingApproval")
            {
                throw new InvalidOperationException(
                    "Chỉ có thể duyệt tài liệu đang chờ duyệt.");
            }

            document.Status = "Approved";
            document.ReviewedBy = reviewerId;
            document.ReviewedAt = DateTime.Now;
            document.RejectionReason = null;

            await _repository.UpdateAsync(document);
        }

        public async Task RejectAsync(
            int documentId,
            int reviewerId,
            string reason)
        {
            var document = await _repository.GetByIdAsync(documentId);

            if (document == null)
            {
                throw new InvalidOperationException("Không tìm thấy tài liệu.");
            }

            if (document.Status != "PendingApproval")
            {
                throw new InvalidOperationException(
                    "Chỉ có thể từ chối tài liệu đang chờ duyệt.");
            }

            document.Status = "Rejected";
            document.ReviewedBy = reviewerId;
            document.ReviewedAt = DateTime.Now;
            document.RejectionReason = string.IsNullOrWhiteSpace(reason)
                ? "Không có lý do cụ thể."
                : reason.Trim();

            await _repository.UpdateAsync(document);
        }

        public async Task DeleteAsync(int id)
        {
            var document = await _repository.GetByIdAsync(id);

            if (document == null)
                return;

            await _repository.DeleteAsync(document);
            await _storage.DeleteIfExistsAsync(document.StoredFileName);
        }

        public async Task<List<Document>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Document?> GetByIdWithChunksAsync(int id)
        {
            return await _repository.GetByIdWithChunksAsync(id);
        }

        public async Task<DocumentDownload?> OpenDownloadAsync(
            int documentId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var document = await _repository.GetByIdAsync(documentId);
            if (document == null ||
                !await _accessPolicy.CanReadAsync(document, user, cancellationToken))
            {
                return null;
            }

            var stream = await _storage.OpenReadAsync(
                document.StoredFileName,
                cancellationToken);
            return stream == null
                ? null
                : new DocumentDownload(
                    stream,
                    document.ContentType,
                    document.OriginalFileName);
        }

        private async Task ValidateSubjectAndChapterAsync(
            int userId,
            int subjectId,
            int chapterId)
        {
            var subject = await _subjectService.GetByIdAsync(subjectId);

            if (subject == null)
            {
                throw new InvalidOperationException(
                    "Môn học không hợp lệ.");
            }

            var chapter = await _chapterService.GetByIdAsync(chapterId);

            if (chapter == null)
            {
                throw new InvalidOperationException(
                    "Chương không hợp lệ.");
            }

            if (chapter.SubjectId != subjectId)
            {
                throw new InvalidOperationException(
                    "Chương đã chọn không thuộc môn học đã chọn.");
            }

            var isAssigned = await _subjectLecturerService
                .IsLecturerAssignedAsync(
                    subjectId,
                    userId);

            if (!isAssigned)
            {
                throw new InvalidOperationException(
                    "Bạn không được phân công vào môn học này nên không thể upload tài liệu.");
            }
        }
    }
}
