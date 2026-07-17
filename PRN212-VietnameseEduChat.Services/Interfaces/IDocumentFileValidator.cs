using Microsoft.AspNetCore.Http;

namespace PRN212_VietnameseEduChat.Services.Interfaces;

public sealed record ValidatedDocumentFile(
    string Extension,
    string ContentType,
    string OriginalFileName,
    long Length);

public interface IDocumentFileValidator
{
    Task<ValidatedDocumentFile> ValidateAsync(
        IFormFile file,
        CancellationToken cancellationToken = default);
}
