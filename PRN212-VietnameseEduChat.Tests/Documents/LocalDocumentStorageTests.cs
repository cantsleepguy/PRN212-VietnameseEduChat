using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PRN212_VietnameseEduChat.Services.Implementations;
using PRN212_VietnameseEduChat.Services.Options;

namespace PRN212_VietnameseEduChat.Tests.Documents;

public sealed class LocalDocumentStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"vietnamese-edu-chat-{Guid.NewGuid():N}");

    [Fact]
    public async Task Save_open_and_delete_round_trip()
    {
        var storage = CreateStorage();
        var source = new MemoryStream("%PDF-test"u8.ToArray());

        var saved = await storage.SaveAsync(source, ".pdf");
        await using (var opened = await storage.OpenReadAsync(saved.StoredFileName))
        {
            Assert.NotNull(opened);
            using var reader = new StreamReader(opened!);
            Assert.Equal("%PDF-test", await reader.ReadToEndAsync());
        }
        Assert.Matches("^[a-f0-9]{32}\\.pdf$", saved.StoredFileName);

        await storage.DeleteIfExistsAsync(saved.StoredFileName);
        await storage.DeleteIfExistsAsync(saved.StoredFileName);
        Assert.Null(await storage.OpenReadAsync(saved.StoredFileName));
    }

    [Theory]
    [InlineData("../secret.pdf")]
    [InlineData("folder/secret.pdf")]
    [InlineData("folder\\secret.pdf")]
    public async Task Open_rejects_names_with_path_segments(string storedName)
    {
        var storage = CreateStorage();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => storage.OpenReadAsync(storedName));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private LocalDocumentStorage CreateStorage()
    {
        var environment = new TestHostEnvironment
        {
            ContentRootPath = _root
        };
        var options = Microsoft.Extensions.Options.Options.Create(new DocumentStorageOptions
        {
            RootPath = "private-documents"
        });
        return new LocalDocumentStorage(environment, options);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
