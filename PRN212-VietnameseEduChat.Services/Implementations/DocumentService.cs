using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repository;
        private readonly IWebHostEnvironment _environment;
        private readonly ITextExtractorService _textExtractorService;
        private readonly IChunkService _chunkService;
        private readonly IEmbeddingService _embeddingService;
        private readonly ISubjectService _subjectService;
        private readonly IChapterService _chapterService;
        private readonly ISubjectLecturerService _subjectLecturerService;
        private readonly IChunkingConfigurationService _chunkingConfigurationService;
        private readonly ISubscriptionService _subscriptionService;

        public DocumentService(
            IDocumentRepository repository,
            IWebHostEnvironment environment,
            ITextExtractorService textExtractorService,
            IChunkService chunkService,
            IEmbeddingService embeddingService,
            ISubjectService subjectService,
            IChapterService chapterService,
            ISubjectLecturerService subjectLecturerService,
            IChunkingConfigurationService chunkingConfigurationService,
            ISubscriptionService subscriptionService)
        {
            _repository = repository;
            _environment = environment;
            _textExtractorService = textExtractorService;
            _chunkService = chunkService;
            _embeddingService = embeddingService;
            _subjectService = subjectService;
            _chapterService = chapterService;
            _subjectLecturerService = subjectLecturerService;
            _chunkingConfigurationService = chunkingConfigurationService;
            _subscriptionService = subscriptionService;
        }

        public async Task UploadAsync(
            IFormFile file,
            int userId,
            int subjectId,
            int chapterId)
        {
            ValidateFile(file);

            await _subscriptionService.EnsureCanUploadDocumentAsync(
                userId,
                file.Length);

            await ValidateSubjectAndChapterAsync(
                userId,
                subjectId,
                chapterId);

            var extension = Path
                .GetExtension(file.FileName)
                .ToLowerInvariant();

            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "documents");

            Directory.CreateDirectory(uploadFolder);

            var storedFileName = $"{Guid.NewGuid():N}{extension}";

            var fullPath = Path.Combine(
                uploadFolder,
                storedFileName);

            await using (var stream = new FileStream(
                fullPath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path
                .Combine("uploads", "documents", storedFileName)
                .Replace("\\", "/");

            var document = new Document
            {
                Title = Path.GetFileNameWithoutExtension(file.FileName),
                OriginalFileName = file.FileName,
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FilePath = relativePath,
                UploadedAt = DateTime.Now,
                UploadedBy = userId,
                SubjectId = subjectId,
                ChapterId = chapterId,
                Status = "Processing",
                TotalChunks = 0
            };

            await _repository.AddAsync(document);

            try
            {
                var text = await _textExtractorService.ExtractAsync(
                    fullPath,
                    extension);

                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException(
                        "Không thể đọc được nội dung từ tài liệu.");
                }

                var chunkEntities = await BuildChunkEntitiesAsync(
                    document.DocumentId,
                    text);

                await _repository.AddChunksAsync(chunkEntities);

                document.TotalChunks = chunkEntities.Count;
                document.Status = "PendingApproval";
                document.ErrorMessage = null;
                document.ReviewedBy = null;
                document.ReviewedAt = null;
                document.RejectionReason = null;

                await _repository.UpdateAsync(document);
            }
            catch (Exception ex)
            {
                document.Status = "Failed";
                document.ErrorMessage = ex.Message;

                await _repository.UpdateAsync(document);

                throw new InvalidOperationException(
                    $"Tài liệu đã được tải lên nhưng index thất bại: {ex.Message}");
            }
        }

        public async Task ReindexAsync(int documentId)
        {
            var document = await _repository.GetByIdAsync(documentId);

            if (document == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy tài liệu.");
            }

            var physicalPath = Path.Combine(
                _environment.WebRootPath,
                document.FilePath.Replace(
                    "/",
                    Path.DirectorySeparatorChar.ToString()));

            if (!File.Exists(physicalPath))
            {
                throw new InvalidOperationException(
                    "Không tìm thấy file gốc của tài liệu để re-index.");
            }

            var previousStatus = document.Status;

            document.Status = "Processing";

            await _repository.UpdateAsync(document);

            try
            {
                var extension = Path
                    .GetExtension(document.StoredFileName)
                    .ToLowerInvariant();

                var text = await _textExtractorService.ExtractAsync(
                    physicalPath,
                    extension);

                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException(
                        "Không thể đọc được nội dung từ tài liệu.");
                }

                var chunkEntities = await BuildChunkEntitiesAsync(
                    document.DocumentId,
                    text);

                await _repository.DeleteChunksByDocumentAsync(
                    document.DocumentId);

                await _repository.AddChunksAsync(chunkEntities);

                document.TotalChunks = chunkEntities.Count;
                document.Status = previousStatus == "Approved"
                    ? "Approved"
                    : "PendingApproval";
                document.ErrorMessage = null;

                await _repository.UpdateAsync(document);
            }
            catch (Exception ex)
            {
                document.Status = "Failed";
                document.ErrorMessage = ex.Message;

                await _repository.UpdateAsync(document);

                throw new InvalidOperationException(
                    $"Re-index tài liệu thất bại: {ex.Message}");
            }
        }

        private async Task<List<DocumentChunk>> BuildChunkEntitiesAsync(
            int documentId,
            string text)
        {
            var activeConfiguration = await _chunkingConfigurationService
                .GetActiveAsync();

            var chunks = _chunkService.Chunk(text, activeConfiguration);

            if (chunks.Count == 0)
            {
                throw new InvalidOperationException(
                    "Không thể chia nội dung tài liệu thành các đoạn nhỏ.");
            }

            var chunkEntities = new List<DocumentChunk>();

            for (int i = 0; i < chunks.Count; i++)
            {
                var embedding = await _embeddingService
                    .CreateEmbeddingAsync(chunks[i].Content);

                var embeddingJson = JsonSerializer.Serialize(embedding);

                chunkEntities.Add(new DocumentChunk
                {
                    DocumentId = documentId,
                    ChunkIndex = i + 1,
                    PageNumber = chunks[i].PageNumber,
                    Content = chunks[i].Content,
                    EmbeddingJson = embeddingJson,
                    EmbeddingModel = _embeddingService.GetModelName(),
                    EmbeddingDimensions = embedding.Length,
                    CreatedAt = DateTime.Now
                });
            }

            return chunkEntities;
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

            var physicalPath = Path.Combine(
                _environment.WebRootPath,
                document.FilePath.Replace(
                    "/",
                    Path.DirectorySeparatorChar.ToString()));

            await _repository.DeleteAsync(document);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
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

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException(
                    "Vui lòng chọn tài liệu cần tải lên.");
            }

            var allowedExtensions = new[]
            {
                ".pdf",
                ".docx",
                ".pptx"
            };

            var extension = Path
                .GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Hệ thống chỉ hỗ trợ file PDF, DOCX và PPTX.");
            }

            const long maxFileSize = 25 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                throw new InvalidOperationException(
                    "Dung lượng tài liệu không được vượt quá 25MB.");
            }
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

            if (!subject.IsActive)
            {
                throw new InvalidOperationException(
                    "Môn học này đang bị ẩn nên không thể upload tài liệu.");
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
