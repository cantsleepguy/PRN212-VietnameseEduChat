using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Options;

namespace PRN212_VietnameseEduChat.Services.Implementations;

public sealed class LocalDocumentStorage : IDocumentStorage
{
    private readonly string _rootPath;

    public LocalDocumentStorage(
        IHostEnvironment environment,
        IOptions<DocumentStorageOptions> options)
    {
        var configuredPath = options.Value.RootPath;
        var candidate = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath);
        _rootPath = Path.GetFullPath(candidate)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public async Task<StoredDocumentFile> SaveAsync(
        Stream content,
        string extension,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(extension) ||
            extension.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]) >= 0)
        {
            throw new InvalidOperationException("Phần mở rộng file không hợp lệ.");
        }

        Directory.CreateDirectory(_rootPath);
        var storedName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = ResolveSafePath(storedName);
        await using var destination = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous);
        await content.CopyToAsync(destination, cancellationToken);
        return new StoredDocumentFile(storedName);
    }

    public Task<Stream?> OpenReadAsync(
        string storedFileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = ResolveSafePath(storedFileName);
        Stream? stream = File.Exists(fullPath)
            ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous)
            : null;
        return Task.FromResult(stream);
    }

    public Task DeleteIfExistsAsync(
        string storedFileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = ResolveSafePath(storedFileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public string GetPhysicalPath(string storedFileName) =>
        ResolveSafePath(storedFileName);

    private string ResolveSafePath(string storedFileName)
    {
        if (string.IsNullOrWhiteSpace(storedFileName) ||
            storedFileName != Path.GetFileName(storedFileName) ||
            storedFileName.Contains('/') ||
            storedFileName.Contains('\\'))
        {
            throw new InvalidOperationException("Tên file lưu trữ không hợp lệ.");
        }

        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, storedFileName));
        var prefix = _rootPath + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Đường dẫn file không hợp lệ.");
        }

        return fullPath;
    }
}
