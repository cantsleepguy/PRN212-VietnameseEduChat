using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<List<Document>> GetAllAsync();

        Task<Document?> GetByIdAsync(int id);

        Task<Document?> GetByIdWithChunksAsync(int id);

        Task UploadAsync(IFormFile file, int userId, int subjectId,
            int chapterId);

        Task ReindexAsync(int documentId);

        Task DeleteAsync(int id);

        Task ApproveAsync(int documentId, int reviewerId);

        Task RejectAsync(int documentId, int reviewerId, string reason);

        Task<DocumentDownload?> OpenDownloadAsync(
            int documentId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }

    public sealed record DocumentDownload(
        Stream Content,
        string ContentType,
        string OriginalFileName);
}
