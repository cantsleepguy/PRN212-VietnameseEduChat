using Microsoft.AspNetCore.Http;
using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.Services.Implementations;

public sealed class DocumentFileValidator : IDocumentFileValidator
{
    private const long MaximumFileSize = 25 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, (string Trusted, string[] Accepted)> TrustedTypes =
        new Dictionary<string, (string Trusted, string[] Accepted)>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = ("application/pdf", ["application/pdf"]),
            [".docx"] = (
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ["application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/zip"]),
            [".pptx"] = (
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ["application/vnd.openxmlformats-officedocument.presentationml.presentation", "application/zip"])
        };

    public async Task<ValidatedDocumentFile> ValidateAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException("Vui lòng chọn tài liệu cần tải lên.");
        }

        if (file.Length > MaximumFileSize)
        {
            throw new InvalidOperationException("Dung lượng tài liệu không được vượt quá 25MB.");
        }

        var safeName = Path.GetFileName(file.FileName.Replace('\\', '/'));
        var extension = Path.GetExtension(safeName).ToLowerInvariant();
        if (!TrustedTypes.TryGetValue(extension, out var typeRule))
        {
            throw new InvalidOperationException("Hệ thống chỉ hỗ trợ file PDF, DOCX và PPTX.");
        }

        if (!typeRule.Accepted.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("MIME type của tài liệu không phù hợp với định dạng file.");
        }

        await using var stream = file.OpenReadStream();
        var header = new byte[5];
        var bytesRead = await stream.ReadAsync(header, cancellationToken);
        var validSignature = extension == ".pdf"
            ? bytesRead >= 5 && header.AsSpan(0, 5).SequenceEqual("%PDF-"u8)
            : bytesRead >= 4 && header.AsSpan(0, 4).SequenceEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 });

        if (!validSignature)
        {
            throw new InvalidOperationException("Nội dung file không đúng với định dạng đã chọn.");
        }

        return new ValidatedDocumentFile(
            extension,
            typeRule.Trusted,
            safeName,
            file.Length);
    }
}
