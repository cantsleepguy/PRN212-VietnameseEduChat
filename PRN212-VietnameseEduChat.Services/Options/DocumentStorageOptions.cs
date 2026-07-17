namespace PRN212_VietnameseEduChat.Services.Options;

public sealed class DocumentStorageOptions
{
    public const string SectionName = "DocumentStorage";

    public string RootPath { get; set; } = Path.Combine("App_Data", "documents");
}
