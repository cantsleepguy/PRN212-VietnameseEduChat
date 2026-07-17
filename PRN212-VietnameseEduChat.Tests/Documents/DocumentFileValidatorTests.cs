using Microsoft.AspNetCore.Http;
using PRN212_VietnameseEduChat.Services.Implementations;

namespace PRN212_VietnameseEduChat.Tests.Documents;

public sealed class DocumentFileValidatorTests
{
    private readonly DocumentFileValidator _validator = new();

    [Fact]
    public async Task Validate_rejects_empty_file()
    {
        var file = CreateFile([], "lesson.pdf", "application/pdf");

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _validator.ValidateAsync(file));

        Assert.Equal("Vui lòng chọn tài liệu cần tải lên.", error.Message);
    }

    [Fact]
    public async Task Validate_rejects_unsupported_extension()
    {
        var file = CreateFile("hello"u8.ToArray(), "lesson.txt", "text/plain");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _validator.ValidateAsync(file));
    }

    [Fact]
    public async Task Validate_rejects_spoofed_pdf()
    {
        var file = CreateFile("not a pdf"u8.ToArray(), "lesson.pdf", "application/pdf");

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _validator.ValidateAsync(file));

        Assert.Contains("nội dung", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Validate_rejects_mismatched_mime_type()
    {
        var file = CreateFile("%PDF-1.7"u8.ToArray(), "lesson.pdf", "text/plain");

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _validator.ValidateAsync(file));

        Assert.Contains("MIME", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("lesson.pdf", "application/pdf", "%PDF-1.7", ".pdf")]
    [InlineData("lesson.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "PKdata", ".docx")]
    [InlineData("lesson.pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation", "PKdata", ".pptx")]
    public async Task Validate_accepts_supported_signature(
        string name,
        string contentType,
        string content,
        string expectedExtension)
    {
        var file = CreateFile(
            System.Text.Encoding.Latin1.GetBytes(content),
            name,
            contentType);

        var result = await _validator.ValidateAsync(file);

        Assert.Equal(expectedExtension, result.Extension);
        Assert.Equal(name, result.OriginalFileName);
    }

    [Fact]
    public async Task Validate_removes_path_from_display_name()
    {
        var file = CreateFile("%PDF-1.7"u8.ToArray(), "../lesson.pdf", "application/pdf");

        var result = await _validator.ValidateAsync(file);

        Assert.Equal("lesson.pdf", result.OriginalFileName);
    }

    private static FormFile CreateFile(byte[] bytes, string name, string contentType)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "UploadFile", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
